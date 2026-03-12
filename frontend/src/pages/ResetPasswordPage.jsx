import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Form, Button, Alert, Spinner } from 'react-bootstrap';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { FiEye, FiEyeOff } from 'react-icons/fi';
import { toast } from 'react-toastify';

import AuthLayout from '../features/auth/components/AuthLayout.jsx';
import {
  resetPasswordThunk,
  selectAuthLoading,
  selectAuthError,
  selectPasswordResetToken,
  clearError,
} from '../store/slices/authSlice.js';

const schema = yup.object({
  newPassword: yup
    .string()
    .min(8, 'Min 8 characters')
    .matches(/[A-Z]/, 'Must contain an uppercase letter')
    .matches(/[0-9]/, 'Must contain a number')
    .matches(/[@$!%*?&#]/, 'Must contain a special character')
    .required('Password is required'),
  confirmPassword: yup
    .string()
    .oneOf([yup.ref('newPassword')], 'Passwords do not match')
    .required('Please confirm your password'),
});

const ResetPasswordPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const loading = useSelector(selectAuthLoading);
  const serverError = useSelector(selectAuthError);
  const passwordResetToken = useSelector(selectPasswordResetToken);

  const [showNew, setShowNew] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({ resolver: yupResolver(schema) });

  // Guard: no token means user came here directly
  if (!passwordResetToken && !success) {
    return (
      <AuthLayout title="Invalid Link" subtitle="This reset link is invalid or has expired.">
        <div className="text-center">
          <Button className="auth-submit-btn" onClick={() => navigate('/auth/forgot-password')}>
            Request a new OTP
          </Button>
        </div>
      </AuthLayout>
    );
  }

  const onSubmit = async (data) => {
    dispatch(clearError());
    const result = await dispatch(
      resetPasswordThunk({
        token: passwordResetToken,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword,
      })
    );

    if (resetPasswordThunk.fulfilled.match(result)) {
      setSuccess(true);
      toast.success('Password reset successfully!');
      setTimeout(() => navigate('/auth/login', { replace: true }), 2500);
    }
  };

  if (success) {
    return (
      <AuthLayout title="Password Updated!">
        <div className="text-center py-2">
          <span className="auth-success-icon">✅</span>
          <p className="text-muted" style={{ fontSize: '0.9rem' }}>
            Your password has been reset. Redirecting you to login...
          </p>
          <Spinner animation="border" size="sm" variant="secondary" className="mt-2" />
        </div>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout
      title="Set new password"
      subtitle="Your new password must be different from your old one"
      footerText="Remember it now?"
      footerLinkText="Sign in"
      footerLinkTo="/auth/login"
    >
      {serverError && (
        <Alert variant="danger" className="auth-alert" onClose={() => dispatch(clearError())} dismissible>
          {serverError}
        </Alert>
      )}

      <Form className="auth-form" onSubmit={handleSubmit(onSubmit)} noValidate>
        {/* New password */}
        <Form.Group className="mb-3">
          <Form.Label>New password</Form.Label>
          <div className="password-input-wrapper">
            <Form.Control
              type={showNew ? 'text' : 'password'}
              placeholder="Create a strong password"
              isInvalid={!!errors.newPassword}
              {...register('newPassword')}
            />
            <button
              type="button"
              className="password-toggle-btn"
              onClick={() => setShowNew((v) => !v)}
              tabIndex={-1}
            >
              {showNew ? <FiEyeOff /> : <FiEye />}
            </button>
            <Form.Control.Feedback type="invalid">
              {errors.newPassword?.message}
            </Form.Control.Feedback>
          </div>
        </Form.Group>

        {/* Confirm password */}
        <Form.Group className="mb-4">
          <Form.Label>Confirm new password</Form.Label>
          <div className="password-input-wrapper">
            <Form.Control
              type={showConfirm ? 'text' : 'password'}
              placeholder="Repeat your new password"
              isInvalid={!!errors.confirmPassword}
              {...register('confirmPassword')}
            />
            <button
              type="button"
              className="password-toggle-btn"
              onClick={() => setShowConfirm((v) => !v)}
              tabIndex={-1}
            >
              {showConfirm ? <FiEyeOff /> : <FiEye />}
            </button>
            <Form.Control.Feedback type="invalid">
              {errors.confirmPassword?.message}
            </Form.Control.Feedback>
          </div>
        </Form.Group>

        <Button type="submit" className="auth-submit-btn" disabled={loading}>
          {loading ? (
            <>
              <Spinner as="span" animation="border" size="sm" className="me-2" />
              Resetting...
            </>
          ) : (
            'Reset Password'
          )}
        </Button>
      </Form>
    </AuthLayout>
  );
};

export default ResetPasswordPage;