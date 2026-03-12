import axiosInstance from './axiosInstance.js';

// GET /api/search?term=shoes&page=1&pageSize=20
export const searchProductsApi = (term, page = 1, pageSize = 20) =>
  axiosInstance.get('/api/search', { params: { term, page, pageSize } });