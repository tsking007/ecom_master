import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Form, Button, Alert, Spinner } from 'react-bootstrap';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { toast } from 'react-toastify';
import { OtpPurpose } from '../constants/otpPurpose.js';

import AuthLayout from '../features/auth/components/AuthLayout.jsx';
import {
  forgotPasswordThunk,
  selectAuthLoading,
  selectAuthError,
  clearError,
  setPendingResetEmail,
} from '../store/slices/authSlice.js';
import { useDispatch as useRawDispatch } from 'react-redux';
import { createAction } from '@reduxjs/toolkit';

const schema = yup.object({
  email: yup.string().email('Enter a valid email').required('Email is required'),
});

const ForgotPasswordPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const loading = useSelector(selectAuthLoading);
  const serverError = useSelector(selectAuthError);
  const [submitted, setSubmitted] = useState(false);

  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors },
  } = useForm({ resolver: yupResolver(schema) });

  const onSubmit = async (data) => {
    dispatch(clearError());
    const result = await dispatch(forgotPasswordThunk({ email: data.email }));

    if (forgotPasswordThunk.fulfilled.match(result)) {
      // Store pendingResetEmail in Redux so VerifyOtpPage can read it
    //   dispatch({ type: 'auth/setPendingResetEmail', payload: data.email });
    dispatch(setPendingResetEmail(data.email));
      setSubmitted(true);
    }
  };

  const handleContinue = () => {
    navigate('/auth/verify-otp', {
      state: { purpose: OtpPurpose.ForgotPassword, context: 'forgot-password' },
    });
  };

  if (submitted) {
    return (
      <AuthLayout
        title="Check your inbox"
        footerText="Remember your password?"
        footerLinkText="Sign in"
        footerLinkTo="/auth/login"
      >
        <div className="text-center py-2">
          <span className="auth-success-icon">📬</span>
          <p className="text-muted mb-4" style={{ fontSize: '0.9rem' }}>
            We sent a 6-digit OTP to <strong>{getValues('email')}</strong>. Enter it on the next step.
          </p>
          <Button className="auth-submit-btn" onClick={handleContinue}>
            Enter OTP
          </Button>
        </div>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout
      title="Forgot password?"
      subtitle="No worries, we'll send you a reset OTP"
      footerText="Remember your password?"
      footerLinkText="Sign in"
      footerLinkTo="/auth/login"
    >
      {serverError && (
        <Alert variant="danger" className="auth-alert" onClose={() => dispatch(clearError())} dismissible>
          {serverError}
        </Alert>
      )}

      <Form className="auth-form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Form.Group className="mb-4">
          <Form.Label>Email address</Form.Label>
          <Form.Control
            type="email"
            placeholder="you@example.com"
            isInvalid={!!errors.email}
            {...register('email')}
          />
          <Form.Control.Feedback type="invalid">
            {errors.email?.message}
          </Form.Control.Feedback>
        </Form.Group>

        <Button type="submit" className="auth-submit-btn" disabled={loading}>
          {loading ? (
            <>
              <Spinner as="span" animation="border" size="sm" className="me-2" />
              Sending OTP...
            </>
          ) : (
            'Send OTP'
          )}
        </Button>
      </Form>
    </AuthLayout>
  );
};

export default ForgotPasswordPage;