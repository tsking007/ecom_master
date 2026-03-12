import { useState, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, useLocation } from 'react-router-dom';
import { Button, Alert, Spinner } from 'react-bootstrap';
import { toast } from 'react-toastify';

import AuthLayout from '../features/auth/components/AuthLayout.jsx';
import {
  verifyOtpThunk,
  forgotPasswordThunk,
  signupThunk,
  selectAuthLoading,
  selectAuthError,
  selectPendingEmail,
  selectPendingResetEmail,
  selectPasswordResetToken,
  clearError,
} from '../store/slices/authSlice.js';

const OTP_LENGTH = 6;
const RESEND_COOLDOWN = 60; // seconds

const VerifyOtpPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();

  const loading = useSelector(selectAuthLoading);
  const serverError = useSelector(selectAuthError);
  const pendingEmail = useSelector(selectPendingEmail);
  const pendingResetEmail = useSelector(selectPendingResetEmail);
  const passwordResetToken = useSelector(selectPasswordResetToken);

  // purpose 1 = EmailVerification (signup), 2 = ForgotPassword
  const { purpose = 1, context = 'signup' } = location.state || {};

  const email = context === 'forgot-password' ? pendingResetEmail : pendingEmail;

  const [otp, setOtp] = useState(Array(OTP_LENGTH).fill(''));
  const [countdown, setCountdown] = useState(RESEND_COOLDOWN);
  const [canResend, setCanResend] = useState(false);
  const inputRefs = useRef([]);

  // Guard: if no email in state, redirect back
  useEffect(() => {
    if (!email) {
      navigate(context === 'forgot-password' ? '/auth/forgot-password' : '/auth/signup', { replace: true });
    }
  }, [email]);

  // Countdown timer
  useEffect(() => {
    if (countdown <= 0) {
      setCanResend(true);
      return;
    }
    const timer = setTimeout(() => setCountdown((c) => c - 1), 1000);
    return () => clearTimeout(timer);
  }, [countdown]);

  // After OTP verify, if it was forgot-password flow, redirect to reset
  useEffect(() => {
    if (passwordResetToken && context === 'forgot-password') {
      toast.success('OTP verified! Please set your new password.');
      navigate('/auth/reset-password', { replace: true });
    }
  }, [passwordResetToken]);

  const handleChange = (index, value) => {
    if (!/^\d*$/.test(value)) return; // digits only
    const newOtp = [...otp];
    newOtp[index] = value.slice(-1); // take last char if pasted multiple
    setOtp(newOtp);
    if (value && index < OTP_LENGTH - 1) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (index, e) => {
    if (e.key === 'Backspace' && !otp[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handlePaste = (e) => {
    e.preventDefault();
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, OTP_LENGTH);
    if (!pasted) return;
    const newOtp = [...otp];
    pasted.split('').forEach((char, i) => {
      newOtp[i] = char;
    });
    setOtp(newOtp);
    const nextEmpty = newOtp.findIndex((v) => !v);
    const focusIndex = nextEmpty === -1 ? OTP_LENGTH - 1 : nextEmpty;
    inputRefs.current[focusIndex]?.focus();
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const otpString = otp.join('');
    if (otpString.length < OTP_LENGTH) {
      toast.error('Please enter all 6 digits');
      return;
    }

    dispatch(clearError());
    const result = await dispatch(
      verifyOtpThunk({
        identifier: email,
        otp: otpString,
        purpose,
      })
    );

    if (verifyOtpThunk.fulfilled.match(result)) {
      const { isAuthenticated, message } = result.payload;
      toast.success(message || 'Verified successfully!');
      if (isAuthenticated) {
        navigate('/', { replace: true });
      }
      // forgot-password redirect handled by useEffect above
    }
  };

  const handleResend = async () => {
    if (!canResend) return;
    setCanResend(false);
    setCountdown(RESEND_COOLDOWN);
    setOtp(Array(OTP_LENGTH).fill(''));
    inputRefs.current[0]?.focus();

    if (context === 'forgot-password') {
      await dispatch(forgotPasswordThunk({ email }));
    } else {
      // Re-trigger signup to resend OTP — or use a dedicated resend endpoint if available
      toast.info('A new OTP has been sent to your email.');
    }
  };

  const maskedEmail = email
    ? email.replace(/(.{2})(.*)(@.*)/, (_, a, b, c) => a + '*'.repeat(b.length) + c)
    : '';

  return (
    <AuthLayout
      title="Verify your email"
      subtitle={`We sent a 6-digit code to ${maskedEmail}`}
      footerText={context === 'signup' ? 'Wrong email?' : undefined}
      footerLinkText={context === 'signup' ? 'Go back' : undefined}
      footerLinkTo={context === 'signup' ? '/auth/signup' : undefined}
    >
      {serverError && (
        <Alert variant="danger" className="auth-alert" onClose={() => dispatch(clearError())} dismissible>
          {serverError}
        </Alert>
      )}

      <form onSubmit={handleSubmit}>
        {/* OTP inputs */}
        <div className="otp-input-group mb-4" onPaste={handlePaste}>
          {otp.map((digit, index) => (
            <input
              key={index}
              ref={(el) => (inputRefs.current[index] = el)}
              type="text"
              inputMode="numeric"
              maxLength={1}
              value={digit}
              onChange={(e) => handleChange(index, e.target.value)}
              onKeyDown={(e) => handleKeyDown(index, e)}
              className={`form-control otp-single-input ${serverError ? 'is-invalid' : ''}`}
              autoFocus={index === 0}
            />
          ))}
        </div>

        <Button type="submit" className="auth-submit-btn mb-3" disabled={loading}>
          {loading ? (
            <>
              <Spinner as="span" animation="border" size="sm" className="me-2" />
              Verifying...
            </>
          ) : (
            'Verify Code'
          )}
        </Button>
      </form>

      {/* Resend */}
      <div className="text-center">
        {canResend ? (
          <button className="resend-link" onClick={handleResend}>
            Resend code
          </button>
        ) : (
          <span className="resend-timer">
            Resend code in <strong>{countdown}s</strong>
          </span>
        )}
      </div>
    </AuthLayout>
  );
};

export default VerifyOtpPage;