import { configureStore } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice.js';
import productReducer from './slices/productSlice.js';
import searchReducer from './slices/searchSlice.js';
import cartReducer from './slices/cartSlice.js';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    products: productReducer,
    search: searchReducer,
    cart: cartReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredPaths: ['auth.accessTokenExpiresAt', 'auth.refreshTokenExpiresAt'],
      },
    }),
});