import axiosInstance from './axiosInstance.js';

const AUTH_BASE = '/api/v1/auth';

// ── Signup ─────────────────────────────────────────────────────────────────────
// Response: { email, message }
export const signupApi = (data) =>
  axiosInstance.post(`${AUTH_BASE}/signup`, data);

// ── Verify OTP ────────────────────────────────────────────────────────────────
// purpose: 1 = EmailVerification, 2 = ForgotPassword (based on your OtpPurpose enum)
// Response: { isAuthenticated, auth, passwordResetToken, message }
export const verifyOtpApi = (data) =>
  axiosInstance.post(`${AUTH_BASE}/verify-otp`, data);

// ── Login ─────────────────────────────────────────────────────────────────────
// Response: { user, accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt }
export const loginApi = (data) =>
  axiosInstance.post(`${AUTH_BASE}/login`, data);

// ── Logout ────────────────────────────────────────────────────────────────────
// Response: 204 No Content
export const logoutApi = () =>
  axiosInstance.post(`${AUTH_BASE}/logout`);

// ── Forgot Password ───────────────────────────────────────────────────────────
// Response: 200 or 204
export const forgotPasswordApi = (data) =>
  axiosInstance.post(`${AUTH_BASE}/forgot-password`, data);

// ── Reset Password ────────────────────────────────────────────────────────────
// Response: 204 No Content
export const resetPasswordApi = (data) =>
  axiosInstance.post(`${AUTH_BASE}/reset-password`, data);

// ── Refresh Token ─────────────────────────────────────────────────────────────
// Response: { accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt }
export const refreshTokenApi = (data) =>
  axiosInstance.post(`${AUTH_BASE}/refresh`, data);

// ── Get Current User ──────────────────────────────────────────────────────────
// Response: UserDto
export const getMeApi = () =>
  axiosInstance.get(`${AUTH_BASE}/me`);