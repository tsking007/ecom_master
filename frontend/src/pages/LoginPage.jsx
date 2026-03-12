import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { Form, Button, Alert, Spinner } from 'react-bootstrap';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { FiEye, FiEyeOff } from 'react-icons/fi';
import { toast } from 'react-toastify';

import AuthLayout from '../features/auth/components/AuthLayout.jsx';
import {
  loginThunk,
  selectAuthLoading,
  selectAuthError,
  clearError,
} from '../store/slices/authSlice.js';

const schema = yup.object({
  email: yup.string().email('Enter a valid email').required('Email is required'),
  password: yup.string().min(6, 'Password must be at least 6 characters').required('Password is required'),
});

const LoginPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const loading = useSelector(selectAuthLoading);
  const serverError = useSelector(selectAuthError);

  const [showPassword, setShowPassword] = useState(false);

  // Redirect to the page the user was trying to access, or home
  const from = location.state?.from?.pathname || '/';

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({ resolver: yupResolver(schema) });

  const onSubmit = async (data) => {
    dispatch(clearError());
    const result = await dispatch(
      loginThunk({
        email: data.email,
        password: data.password,
        deviceInfo: navigator.userAgent,
      })
    );

    if (loginThunk.fulfilled.match(result)) {
      toast.success(`Welcome back, ${result.payload.user.firstName}!`);
      navigate(from, { replace: true });
    }
  };

  return (
    <AuthLayout
      title="Welcome back"
      subtitle="Sign in to your account to continue"
      footerText="Don't have an account?"
      footerLinkText="Sign up"
      footerLinkTo="/auth/signup"
    >
      {serverError && (
        <Alert variant="danger" className="auth-alert" onClose={() => dispatch(clearError())} dismissible>
          {serverError}
        </Alert>
      )}

      <Form className="auth-form" onSubmit={handleSubmit(onSubmit)} noValidate>
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

        {/* Password */}
        <Form.Group className="mb-2">
          <Form.Label>Password</Form.Label>
          <div className="password-input-wrapper">
            <Form.Control
              type={showPassword ? 'text' : 'password'}
              placeholder="Enter your password"
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

        {/* Forgot password */}
        <div className="text-end mb-4">
          <Link to="/auth/forgot-password" className="text-decoration-none" style={{ fontSize: '0.82rem', color: '#667eea' }}>
            Forgot password?
          </Link>
        </div>

        {/* Submit */}
        <Button
          type="submit"
          className="auth-submit-btn"
          disabled={loading}
        >
          {loading ? (
            <>
              <Spinner as="span" animation="border" size="sm" className="me-2" />
              Signing in...
            </>
          ) : (
            'Sign In'
          )}
        </Button>
      </Form>
    </AuthLayout>
  );
};

export default LoginPage;