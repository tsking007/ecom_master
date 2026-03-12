import { useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { setCredentials, logout } from '../store/slices/authSlice.js';
import { refreshTokenApi } from '../api/authApi.js';
import { fetchCurrentUserThunk } from '../store/slices/authSlice.js';

const REFRESH_BUFFER_MS = 60 * 1000; // refresh 1 minute before expiry

export const useTokenRefresh = () => {
  const dispatch = useDispatch();
  const { accessToken, refreshToken, accessTokenExpiresAt, isAuthenticated } = useSelector(
    (state) => state.auth
  );
  const timerRef = useRef(null);

  // On mount, if we have a token from localStorage, validate it by fetching /me
  useEffect(() => {
    if (accessToken && !isAuthenticated) {
      dispatch(fetchCurrentUserThunk());
    }
  }, []);

  useEffect(() => {
    if (!accessToken || !accessTokenExpiresAt || !refreshToken) return;

    const expiresAt = new Date(accessTokenExpiresAt).getTime();
    const now = Date.now();
    const delay = expiresAt - now - REFRESH_BUFFER_MS;

    if (delay <= 0) {
      // Token already expired or about to — refresh immediately
      handleRefresh();
      return;
    }

    timerRef.current = setTimeout(handleRefresh, delay);

    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [accessToken, accessTokenExpiresAt, refreshToken]);

  const handleRefresh = async () => {
    try {
      const response = await refreshTokenApi({ accessToken, refreshToken });
      dispatch(setCredentials(response.data));
    } catch {
      dispatch(logout());
    }
  };
};