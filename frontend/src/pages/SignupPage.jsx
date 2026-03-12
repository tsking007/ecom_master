import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Form, Button, Alert, Spinner, Row, Col } from 'react-bootstrap';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { FiEye, FiEyeOff } from 'react-icons/fi';
import { OtpPurpose } from '../constants/otpPurpose.js';

import AuthLayout from '../features/auth/components/AuthLayout.jsx';
import {
  signupThunk,
  selectAuthLoading,
  selectAuthError,
  clearError,
} from '../store/slices/authSlice.js';

const schema = yup.object({
  firstName: yup.string().min(2, 'Min 2 characters').required('First name is required'),
  lastName: yup.string().min(2, 'Min 2 characters').required('Last name is required'),
  email: yup.string().email('Enter a valid email').required('Email is required'),
  phoneNumber: yup
    .string()
    .matches(/^[0-9]{7,15}$/, 'Enter a valid phone number')
    .required('Phone number is required'),
  password: yup
    .string()
    .min(8, 'Min 8 characters')
    .matches(/[A-Z]/, 'Must contain an uppercase letter')
    .matches(/[0-9]/, 'Must contain a number')
    .matches(/[@$!%*?&#]/, 'Must contain a special character (@$!%*?&#)')
    .required('Password is required'),
  confirmPassword: yup
    .string()
    .oneOf([yup.ref('password')], 'Passwords do not match')
    .required('Please confirm your password'),
});

const SignupPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const loading = useSelector(selectAuthLoading);
  const serverError = useSelector(selectAuthError);

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({ resolver: yupResolver(schema) });

  const onSubmit = async (data) => {
    dispatch(clearError());
    const result = await dispatch(
      signupThunk({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
        confirmPassword: data.confirmPassword,
        phoneNumber: data.phoneNumber,
      })
    );

    if (signupThunk.fulfilled.match(result)) {
      // pendingEmail is now set in Redux; navigate to OTP page
      navigate('/auth/verify-otp', {
        state: { purpose: OtpPurpose.EmailVerification, context: 'signup' },
      });
    }
  };

  return (
    <AuthLayout
      title="Create your account"
      subtitle="Join us today — it's free"
      footerText="Already have an account?"
      footerLinkText="Sign in"
      footerLinkTo="/auth/login"
    >
      {serverError && (
        <Alert variant="danger" className="auth-alert" onClose={() => dispatch(clearError())} dismissible>
          {serverError}
        </Alert>
      )}

      <Form className="auth-form" onSubmit={handleSubmit(onSubmit)} noValidate>
        {/* Name row */}
        <Row className="g-2 mb-3">
          <Col xs={6}>
            <Form.Group>
              <Form.Label>First name</Form.Label>
              <Form.Control
                type="text"
                placeholder="John"
                isInvalid={!!errors.firstName}
                {...register('firstName')}
              />
              <Form.Control.Feedback type="invalid">
                {errors.firstName?.message}
              </Form.Control.Feedback>
            </Form.Group>
          </Col>
          <Col xs={6}>
            <Form.Group>
              <Form.Label>Last name</Form.Label>
              <Form.Control
                type="text"
                placeholder="Doe"
                isInvalid={!!errors.lastName}
                {...register('lastName')}
              />
              <Form.Control.Feedback type="invalid">
                {errors.lastName?.message}
              </Form.Control.Feedback>
            </Form.Group>
          </Col>
        </Row>

        {/* Email */}
        <Form.Group className="mb-3">
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

        {/* Phone */}
        <Form.Group className="mb-3">
          <Form.Label>Phone number</Form.Label>
          <Form.Control
            type="tel"
            placeholder="9876543210"
            isInvalid={!!errors.phoneNumber}
            {...register('phoneNumber')}
          />
          <Form.Control.Feedback type="invalid">
            {errors.phoneNumber?.message}
          </Form.Control.Feedback>
        </Form.Group>

        {/* Password */}
        <Form.Group className="mb-3">
          <Form.Label>Password</Form.Label>
          <div className="password-input-wrapper">
            <Form.Control
              type={showPassword ? 'text' : 'password'}
              placeholder="Create a strong password"
              isInvalid={!!errors.password}
              {...register('password')}
            />
            <button
              type="button"
              className="password-toggle-btn"
              onClick={() => setShowPassword((v) => !v)}
              tabIndex={-1}
            >
              {showPassword ? <FiEyeOff /> : <FiEye />}
            </button>
            <Form.Control.Feedback type="invalid">
              {errors.password?.message}
            </Form.Control.Feedback>
          </div>
        </Form.Group>

        {/* Confirm Password */}
        <Form.Group className="mb-4">
          <Form.Label>Confirm password</Form.Label>
          <div className="password-input-wrapper">
            <Form.Control
              type={showConfirm ? 'text' : 'password'}
              placeholder="Repeat your password"
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
              Creating account...
            </>
          ) : (
            'Create Account'
          )}
        </Button>
      </Form>
    </AuthLayout>
  );
};

export default SignupPage;