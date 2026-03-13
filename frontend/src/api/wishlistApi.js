import axiosInstance from './axiosInstance.js';

const WISHLIST_BASE = '/api/v1/wishlist';

// GET /api/v1/wishlist
// Response: WishlistDto
export const getWishlistApi = () =>
  axiosInstance.get(WISHLIST_BASE);

// POST /api/v1/wishlist  { productId }
export const addToWishlistApi = (productId) =>
  axiosInstance.post(WISHLIST_BASE, { productId });

// DELETE /api/v1/wishlist/{wishlistId}
export const removeWishlistItemApi = (wishlistId) =>
  axiosInstance.delete(`${WISHLIST_BASE}/${wishlistId}`);

// POST /api/v1/wishlist/{wishlistId}/move-to-cart
export const moveToCartApi = (wishlistId) =>
  axiosInstance.post(`${WISHLIST_BASE}/${wishlistId}/move-to-cart`);