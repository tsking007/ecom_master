import axiosInstance from './axiosInstance.js';

const PRODUCTS_BASE = '/api/products';

// GET /api/products?pageNumber=1&pageSize=20&category=...&brand=...etc
export const getProductsApi = (params = {}) =>
  axiosInstance.get(PRODUCTS_BASE, { params });

// GET /api/products/categories
export const getCategoriesApi = () =>
  axiosInstance.get(`${PRODUCTS_BASE}/categories`);

// GET /api/products/bestsellers?count=8
export const getBestsellersApi = (count = 8) =>
  axiosInstance.get(`${PRODUCTS_BASE}/bestsellers`, { params: { count } });

// GET /api/products/:slug
export const getProductBySlugApi = (slug) =>
  axiosInstance.get(`${PRODUCTS_BASE}/${slug}`);