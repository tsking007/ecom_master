import axiosInstance from './axiosInstance.js';

const ORDERS_BASE = '/api/v1/orders';

// GET /api/v1/orders/{orderId}
// Response: OrderDetailsDto
export const getOrderDetailsApi = (orderId) =>
  axiosInstance.get(`${ORDERS_BASE}/${orderId}`);

export const getUserOrdersApi = (pageNumber = 1, pageSize = 10) =>
  axiosInstance.get('/api/v1/orders', {
    params: { pageNumber, pageSize }
  });