import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { selectIsAuthenticated } from '../store/slices/authSlice.js';
import ProtectedRoute from './ProtectedRoute.jsx';

import LoginPage from '../pages/LoginPage.jsx';
import SignupPage from '../pages/SignupPage.jsx';
import VerifyOtpPage from '../pages/VerifyOtpPage.jsx';
import ForgotPasswordPage from '../pages/ForgotPasswordPage.jsx';
import ResetPasswordPage from '../pages/ResetPasswordPage.jsx';

// Placeholder pages — will be replaced as we build them out
const HomePage = () => <div className="p-5 text-center"><h2>Home Page (Coming Soon)</h2></div>;
const DashboardPage = () => <div className="p-5 text-center"><h2>Dashboard (Protected)</h2></div>;

// Redirect authenticated users away from auth pages
const GuestRoute = ({ children }) => {
  const isAuthenticated = useSelector(selectIsAuthenticated);
  return isAuthenticated ? <Navigate to="/" replace /> : children;
};

const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route path="/" element={<HomePage />} />

        {/* Auth routes — redirect away if already logged in */}
        <Route
          path="/auth/login"
          element={
            <GuestRoute>
              <LoginPage />
            </GuestRoute>
          }
        />
        <Route
          path="/auth/signup"
          element={
            <GuestRoute>
              <SignupPage />
            </GuestRoute>
          }
        />
        <Route
          path="/auth/verify-otp"
          element={<VerifyOtpPage />}
        />
        <Route
          path="/auth/forgot-password"
          element={
            <GuestRoute>
              <ForgotPasswordPage />
            </GuestRoute>
          }
        />
        <Route
          path="/auth/reset-password"
          element={<ResetPasswordPage />}
        />

        {/* Protected routes */}
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />

        {/* Catch-all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
};

export default AppRouter;