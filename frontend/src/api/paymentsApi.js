import axiosInstance from './axiosInstance.js';
import { v4 as uuidv4 } from 'uuid';

const PAYMENTS_BASE = '/api/v1/payments';

// Generates a key that is stable for this browser tab's checkout session.
// Format: idem_<uuid> — simple, collision-proof, no PII.
export const generateIdempotencyKey = () => `idem_${uuidv4()}`;

// POST /api/v1/payments/create-checkout-session
// Body:    { addressId: Guid | null }
// Header:  Idempotency-Key: idem_<uuid>
// Response: { orderId, orderNumber, sessionId, sessionUrl }
export const createCheckoutSessionApi = (addressId, idempotencyKey) =>
  axiosInstance.post(
    `${PAYMENTS_BASE}/create-checkout-session`,
    { addressId },
    {
      headers: {
        // If no key is passed the header is simply omitted — safe fallback
        ...(idempotencyKey && { 'Idempotency-Key': idempotencyKey }),
      },
    }
  );

// GET /api/v1/payments/session/{sessionId}
export const getSessionStatusApi = (sessionId) =>
  axiosInstance.get(`${PAYMENTS_BASE}/session/${sessionId}`);

// POST /api/v1/payments/mock/checkout-session-completed/{sessionId}
export const mockConfirmSessionApi = (sessionId) =>
  axiosInstance.post(`${PAYMENTS_BASE}/mock/checkout-session-completed/${sessionId}`);