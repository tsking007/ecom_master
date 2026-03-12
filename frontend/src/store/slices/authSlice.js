import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import {
  loginApi,
  logoutApi,
  signupApi,
  verifyOtpApi,
  forgotPasswordApi,
  resetPasswordApi,
  getMeApi,
} from '../../api/authApi.js';

// ── Helper: persist tokens to localStorage ────────────────────────────────────
const saveTokens = ({ accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt }) => {
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refreshToken);
  localStorage.setItem('accessTokenExpiresAt', accessTokenExpiresAt);
  localStorage.setItem('refreshTokenExpiresAt', refreshTokenExpiresAt);
};

const clearTokens = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('accessTokenExpiresAt');
  localStorage.removeItem('refreshTokenExpiresAt');
};

// ── Thunks ────────────────────────────────────────────────────────────────────

export const loginThunk = createAsyncThunk(
  'auth/login',
  async (credentials, { rejectWithValue }) => {
    try {
      const response = await loginApi(credentials);
      return response.data;
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || err.response?.data?.title || 'Login failed'
      );
    }
  }
);

export const signupThunk = createAsyncThunk(
  'auth/signup',
  async (userData, { rejectWithValue }) => {
    try {
      const response = await signupApi(userData);
      return response.data; // { email, message }
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || err.response?.data?.title || 'Signup failed'
      );
    }
  }
);

export const verifyOtpThunk = createAsyncThunk(
  'auth/verifyOtp',
  async (otpData, { rejectWithValue }) => {
    try {
      const response = await verifyOtpApi(otpData);
      return response.data; // { isAuthenticated, auth, passwordResetToken, message }
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || err.response?.data?.title || 'OTP verification failed'
      );
    }
  }
);

export const logoutThunk = createAsyncThunk(
  'auth/logout',
  async (_, { rejectWithValue }) => {
    try {
      await logoutApi();
    } catch (err) {
      // Always clear locally even if server call fails
    }
  }
);

export const forgotPasswordThunk = createAsyncThunk(
  'auth/forgotPassword',
  async (data, { rejectWithValue }) => {
    try {
      await forgotPasswordApi(data);
      return true;
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || err.response?.data?.title || 'Request failed'
      );
    }
  }
);

export const resetPasswordThunk = createAsyncThunk(
  'auth/resetPassword',
  async (data, { rejectWithValue }) => {
    try {
      await resetPasswordApi(data);
      return true;
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || err.response?.data?.title || 'Reset failed'
      );
    }
  }
);

export const fetchCurrentUserThunk = createAsyncThunk(
  'auth/fetchCurrentUser',
  async (_, { rejectWithValue }) => {
    try {
      const response = await getMeApi();
      return response.data;
    } catch (err) {
      return rejectWithValue('Session expired');
    }
  }
);

// ── Initial State ─────────────────────────────────────────────────────────────
const initialState = {
  user: null,
  accessToken: localStorage.getItem('accessToken') || null,
  refreshToken: localStorage.getItem('refreshToken') || null,
  accessTokenExpiresAt: localStorage.getItem('accessTokenExpiresAt') || null,
  refreshTokenExpiresAt: localStorage.getItem('refreshTokenExpiresAt') || null,
  isAuthenticated: !!localStorage.getItem('accessToken'),

  // Signup flow: temporarily hold email for OTP step
  pendingEmail: null,
  // Forgot password flow: hold email for OTP step
  pendingResetEmail: null,
  // Reset password flow: hold token from OTP verify
  passwordResetToken: null,

  loading: false,
  error: null,
};

// ── Slice ─────────────────────────────────────────────────────────────────────
const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    // Called by axios interceptor after silent refresh
    // Inside authSlice reducers:
    setPendingResetEmail: (state, action) => {
        state.pendingResetEmail = action.payload;
    },
    setCredentials: (state, action) => {
      const { accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt } = action.payload;
      state.accessToken = accessToken;
      state.refreshToken = refreshToken;
      state.accessTokenExpiresAt = accessTokenExpiresAt;
      state.refreshTokenExpiresAt = refreshTokenExpiresAt;
      saveTokens(action.payload);
    },
    logout: (state) => {
      state.user = null;
      state.accessToken = null;
      state.refreshToken = null;
      state.accessTokenExpiresAt = null;
      state.refreshTokenExpiresAt = null;
      state.isAuthenticated = false;
      state.pendingEmail = null;
      state.pendingResetEmail = null;
      state.passwordResetToken = null;
      state.error = null;
      clearTokens();
    },
    clearError: (state) => {
      state.error = null;
    },
    clearPendingState: (state) => {
      state.pendingEmail = null;
      state.pendingResetEmail = null;
      state.passwordResetToken = null;
    },
  },
  extraReducers: (builder) => {
    // ── Login ──────────────────────────────────────────────────────────────────
    builder
      .addCase(loginThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(loginThunk.fulfilled, (state, action) => {
        state.loading = false;
        const { user, accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt } = action.payload;
        state.user = user;
        state.accessToken = accessToken;
        state.refreshToken = refreshToken;
        state.accessTokenExpiresAt = accessTokenExpiresAt;
        state.refreshTokenExpiresAt = refreshTokenExpiresAt;
        state.isAuthenticated = true;
        saveTokens({ accessToken, refreshToken, accessTokenExpiresAt, refreshTokenExpiresAt });
      })
      .addCase(loginThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });

    // ── Signup ─────────────────────────────────────────────────────────────────
    builder
      .addCase(signupThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(signupThunk.fulfilled, (state, action) => {
        state.loading = false;
        state.pendingEmail = action.payload.email;
      })
      .addCase(signupThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });

    // ── Verify OTP ─────────────────────────────────────────────────────────────
    builder
      .addCase(verifyOtpThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(verifyOtpThunk.fulfilled, (state, action) => {
        state.loading = false;
        const { isAuthenticated, auth, passwordResetToken } = action.payload;
        if (isAuthenticated && auth) {
          state.user = auth.user;
          state.accessToken = auth.accessToken;
          state.refreshToken = auth.refreshToken;
          state.accessTokenExpiresAt = auth.accessTokenExpiresAt;
          state.refreshTokenExpiresAt = auth.refreshTokenExpiresAt;
          state.isAuthenticated = true;
          state.pendingEmail = null;
          saveTokens({
            accessToken: auth.accessToken,
            refreshToken: auth.refreshToken,
            accessTokenExpiresAt: auth.accessTokenExpiresAt,
            refreshTokenExpiresAt: auth.refreshTokenExpiresAt,
          });
        }
        // Forgot password OTP flow — store reset token
        if (passwordResetToken) {
          state.passwordResetToken = passwordResetToken;
          state.pendingResetEmail = null;
        }
      })
      .addCase(verifyOtpThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });

    // ── Logout ─────────────────────────────────────────────────────────────────
    builder
      .addCase(logoutThunk.fulfilled, (state) => {
        state.user = null;
        state.accessToken = null;
        state.refreshToken = null;
        state.accessTokenExpiresAt = null;
        state.refreshTokenExpiresAt = null;
        state.isAuthenticated = false;
        state.error = null;
        clearTokens();
      });

    // ── Forgot Password ────────────────────────────────────────────────────────
    builder
      .addCase(forgotPasswordThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(forgotPasswordThunk.fulfilled, (state, action) => {
        state.loading = false;
        // pendingResetEmail is set before dispatch in the component
      })
      .addCase(forgotPasswordThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });

    // ── Reset Password ─────────────────────────────────────────────────────────
    builder
      .addCase(resetPasswordThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(resetPasswordThunk.fulfilled, (state) => {
        state.loading = false;
        state.passwordResetToken = null;
      })
      .addCase(resetPasswordThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });

    // ── Fetch Current User ─────────────────────────────────────────────────────
    builder
      .addCase(fetchCurrentUserThunk.fulfilled, (state, action) => {
        state.user = action.payload;
        state.isAuthenticated = true;
      })
      .addCase(fetchCurrentUserThunk.rejected, (state) => {
        state.user = null;
        state.isAuthenticated = false;
        clearTokens();
      });
  },
});

export const { setCredentials, logout, clearError, clearPendingState,setPendingResetEmail } = authSlice.actions;
export default authSlice.reducer;

// ── Selectors ─────────────────────────────────────────────────────────────────
export const selectCurrentUser = (state) => state.auth.user;
export const selectIsAuthenticated = (state) => state.auth.isAuthenticated;
export const selectAuthLoading = (state) => state.auth.loading;
export const selectAuthError = (state) => state.auth.error;
export const selectPendingEmail = (state) => state.auth.pendingEmail;
export const selectPendingResetEmail = (state) => state.auth.pendingResetEmail;
export const selectPasswordResetToken = (state) => state.auth.passwordResetToken;
export const selectAccessToken = (state) => state.auth.accessToken;