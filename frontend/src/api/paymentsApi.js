import axiosInstance from './axiosInstance.js';

const PAYMENTS_BASE = '/api/v1/payments';

// POST /api/v1/payments/create-checkout-session
// Body: { addressId: Guid | null }
// Response: { orderId, orderNumber, sessionId, sessionUrl }
export const createCheckoutSessionApi = (addressId) =>
  axiosInstance.post(`${PAYMENTS_BASE}/create-checkout-session`, { addressId });

// GET /api/v1/payments/session/{sessionId}
// Response: CheckoutSessionStatusDto
export const getSessionStatusApi = (sessionId) =>
  axiosInstance.get(`${PAYMENTS_BASE}/session/${sessionId}`);

// POST /api/v1/payments/mock/checkout-session-completed/{sessionId}
export const mockConfirmSessionApi = (sessionId) =>
  axiosInstance.post(`${PAYMENTS_BASE}/mock/checkout-session-completed/${sessionId}`);