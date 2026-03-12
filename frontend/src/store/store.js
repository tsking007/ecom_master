import { configureStore } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice.js';

export const store = configureStore({
  reducer: {
    auth: authReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        // Ignore date strings in auth state
        ignoredPaths: ['auth.accessTokenExpiresAt', 'auth.refreshTokenExpiresAt'],
      },
    }),
});