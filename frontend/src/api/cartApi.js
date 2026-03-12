import axiosInstance from './axiosInstance.js';

const CART_BASE = '/api/v1/cart';

// GET /api/v1/cart
export const getCartApi = () =>
  axiosInstance.get(CART_BASE);

// POST /api/v1/cart  { productId, quantity }
export const addToCartApi = (productId, quantity) =>
  axiosInstance.post(CART_BASE, { productId, quantity });

// PUT /api/v1/cart/:cartItemId  { quantity }
export const updateCartItemApi = (cartItemId, quantity) =>
  axiosInstance.put(`${CART_BASE}/${cartItemId}`, { quantity });

// DELETE /api/v1/cart/:cartItemId
export const removeCartItemApi = (cartItemId) =>
  axiosInstance.delete(`${CART_BASE}/${cartItemId}`);

// DELETE /api/v1/cart
export const clearCartApi = () =>
  axiosInstance.delete(CART_BASE);