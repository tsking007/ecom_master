import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { selectIsAuthenticated } from '../store/slices/authSlice.js';
import ProtectedRoute from './ProtectedRoute.jsx';

// Auth pages
import LoginPage from '../pages/LoginPage.jsx';
import SignupPage from '../pages/SignupPage.jsx';
import VerifyOtpPage from '../pages/VerifyOtpPage.jsx';
import ForgotPasswordPage from '../pages/ForgotPasswordPage.jsx';
import ResetPasswordPage from '../pages/ResetPasswordPage.jsx';

// Main pages
import HomePage from '../pages/HomePage.jsx';
import ProductListPage from '../pages/ProductListPage.jsx';
import ProductDetailPage from '../pages/ProductDetailPage.jsx';
import SearchResultsPage from '../pages/SearchResultsPage.jsx';
import CartPage from '../pages/CartPage.jsx';

// Redirect authenticated users away from auth-only pages
const GuestRoute = ({ children }) => {
  const isAuthenticated = useSelector(selectIsAuthenticated);
  return isAuthenticated ? <Navigate to="/" replace /> : children;
};

const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>
        {/* ── Public ───────────────────────────────────────────── */}
        <Route path="/" element={<HomePage />} />
        <Route path="/products" element={<ProductListPage />} />
        <Route path="/products/:slug" element={<ProductDetailPage />} />
        <Route path="/search" element={<SearchResultsPage />} />

        {/* ── Auth (guest only) ─────────────────────────────────── */}
        <Route path="/auth/login" element={<GuestRoute><LoginPage /></GuestRoute>} />
        <Route path="/auth/signup" element={<GuestRoute><SignupPage /></GuestRoute>} />
        <Route path="/auth/verify-otp" element={<VerifyOtpPage />} />
        <Route path="/auth/forgot-password" element={<GuestRoute><ForgotPasswordPage /></GuestRoute>} />
        <Route path="/auth/reset-password" element={<ResetPasswordPage />} />

        {/* ── Protected ─────────────────────────────────────────── */}
        <Route path="/cart" element={<ProtectedRoute><CartPage /></ProtectedRoute>} />

        {/* ── Catch-all ─────────────────────────────────────────── */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
};

export default AppRouter;