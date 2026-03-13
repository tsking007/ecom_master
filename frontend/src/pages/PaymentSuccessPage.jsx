import { useEffect, useState, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Container, Card, Button, Spinner, Badge, Table, Image } from 'react-bootstrap';
import { FiCheckCircle, FiPackage, FiArrowRight } from 'react-icons/fi';
import { toast } from 'react-toastify';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import { mockConfirmSessionApi, getSessionStatusApi } from '../api/paymentsApi.js';
import { getOrderDetailsApi } from '../api/ordersApi.js';
import { useDispatch } from 'react-redux';
import { resetCart } from '../store/slices/cartSlice.js';

const PLACEHOLDER = 'https://placehold.co/56x56?text=?';

const PaymentSuccessPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const sessionId = searchParams.get('session_id');

  const [status, setStatus] = useState(null);   // CheckoutSessionStatusDto
  const [order, setOrder] = useState(null);      // OrderDetailsDto
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const hasFetched = useRef(false);

  useEffect(() => {
    if (!sessionId) {
      navigate('/');
      return;
    }
    if (hasFetched.current) return;
    hasFetched.current = true;

    const verify = async () => {
      try {

        await mockConfirmSessionApi(sessionId);

        // 1. Poll session status
        const statusRes = await getSessionStatusApi(sessionId);
        const sessionStatus = statusRes.data;
        setStatus(sessionStatus);

        if (sessionStatus.isPaid) {
          // 2. Fetch full order details
          const orderRes = await getOrderDetailsApi(sessionStatus.orderId);
          setOrder(orderRes.data);
          // 3. Clear cart from Redux (backend already cleared it)
          dispatch(resetCart());
        } else {
          // Payment not confirmed yet — redirect to failure
          navigate(`/payment/failure?session_id=${sessionId}`);
        }
      } catch (err) {
        setError('Could not verify payment. Please check your orders.');
        toast.error('Could not verify payment');
      } finally {
        setLoading(false);
      }
    };

    verify();
  }, [sessionId, navigate, dispatch]);

  if (loading) {
    return (
      <>
        <AppNavbar />
        <div className="d-flex flex-column justify-content-center align-items-center" style={{ minHeight: '60vh' }}>
          <Spinner animation="border" variant="success" className="mb-3" />
          <p className="text-muted">Verifying your payment…</p>
        </div>
        <Footer />
      </>
    );
  }

  if (error) {
    return (
      <>
        <AppNavbar />
        <Container className="py-5 text-center">
          <p className="text-danger">{error}</p>
          <Button variant="primary" onClick={() => navigate('/profile?tab=orders')}>
            View My Orders
          </Button>
        </Container>
        <Footer />
      </>
    );
  }

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container className="py-5 flex-grow-1" style={{ maxWidth: '680px' }}>
        {/* Success header */}
        <div className="text-center mb-4">
          <FiCheckCircle size={56} className="text-success mb-3" />
          <h3 className="fw-bold text-success">Payment Successful!</h3>
          <p className="text-muted">
            Your order <strong>#{order?.orderNumber || status?.orderNumber}</strong> has been placed.
          </p>
        </div>

        {order && (
          <Card className="border-0 shadow-sm mb-4">
            <Card.Header className="bg-white fw-bold d-flex justify-content-between align-items-center">
              <span><FiPackage className="me-2" />Order Details</span>
              <div className="d-flex gap-2">
                <Badge bg="success" pill>{order.paymentStatus}</Badge>
                <Badge bg="primary" pill>{order.trackingStatus}</Badge>
              </div>
            </Card.Header>
            <Card.Body className="p-0">
              {/* Items */}
              {order.items.map((item, idx) => (
                <div
                  key={idx}
                  className={`px-3 py-2 d-flex align-items-center gap-3 ${
                    idx < order.items.length - 1 ? 'border-bottom' : ''
                  }`}
                >
                  <Image
                    src={item.productImageUrl || PLACEHOLDER}
                    alt={item.productName}
                    rounded
                    style={{ width: '52px', height: '52px', objectFit: 'cover', flexShrink: 0 }}
                    onError={(e) => { e.target.src = PLACEHOLDER; }}
                  />
                  <div className="flex-grow-1">
                    <div className="fw-semibold small">{item.productName}</div>
                    <div className="text-muted" style={{ fontSize: '0.75rem' }}>
                      Qty: {item.quantity} ×{' '}
                      ₹{(item.discountedUnitPrice ?? item.unitPrice)?.toLocaleString('en-IN')}
                    </div>
                  </div>
                  <div className="fw-semibold small text-nowrap">
                    ₹{item.totalPrice?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                  </div>
                </div>
              ))}

              {/* Totals */}
              <div className="px-3 py-3 border-top">
                <Table borderless size="sm" className="mb-0">
                  <tbody>
                    <tr>
                      <td className="text-muted small">Subtotal</td>
                      <td className="text-end small">
                        ₹{order.subTotal?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                      </td>
                    </tr>
                    {order.discountAmount > 0 && (
                      <tr>
                        <td className="text-muted small">Discount</td>
                        <td className="text-end small text-success">
                          −₹{order.discountAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    )}
                    {order.shippingAmount > 0 && (
                      <tr>
                        <td className="text-muted small">Shipping</td>
                        <td className="text-end small">
                          ₹{order.shippingAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    )}
                    {order.taxAmount > 0 && (
                      <tr>
                        <td className="text-muted small">Tax</td>
                        <td className="text-end small">
                          ₹{order.taxAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    )}
                    <tr>
                      <td colSpan={2}><hr className="my-1" /></td>
                    </tr>
                    <tr>
                      <td className="fw-bold">Total Paid</td>
                      <td className="text-end fw-bold text-primary fs-6">
                        ₹{order.totalAmount?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                      </td>
                    </tr>
                  </tbody>
                </Table>
              </div>

              {/* Shipping address snapshot */}
              {order.shippingAddressSnapshot && (() => {
                let addr = null;
                try {
                  addr = typeof order.shippingAddressSnapshot === 'string'
                    ? JSON.parse(order.shippingAddressSnapshot)
                    : order.shippingAddressSnapshot;
                } catch {
                  return (
                    <div className="mt-3 border-top pt-3">
                      <div className="text-muted small fw-semibold mb-1">Shipped To:</div>
                      <div className="text-muted small">{order.shippingAddressSnapshot}</div>
                    </div>
                  );
                }
                return (
                  <div className="mt-3 border-top pt-3">
                    <div className="text-muted small fw-semibold mb-1">Shipped To:</div>
                    <div className="text-muted small">
                      <div>{addr.FullName} {addr.PhoneNumber && `· ${addr.PhoneNumber}`}</div>
                      <div>{addr.AddressLine1}{addr.AddressLine2 && `, ${addr.AddressLine2}`}</div>
                      <div>{addr.City}, {addr.State} — {addr.PostalCode}</div>
                      <div>{addr.Country}</div>
                    </div>
                  </div>
                );
              })()}
            </Card.Body>
          </Card>
        )}

        {/* CTA buttons */}
        <div className="d-flex gap-3 justify-content-center flex-wrap">
          <Button
            variant="primary"
            onClick={() => navigate('/profile?tab=orders')}
            className="d-flex align-items-center gap-2"
          >
            <FiPackage /> View All Orders
          </Button>
          <Button
            variant="outline-secondary"
            onClick={() => navigate('/products')}
            className="d-flex align-items-center gap-2"
          >
            Continue Shopping <FiArrowRight />
          </Button>
        </div>
      </Container>

      <Footer />
    </div>
  );
};

export default PaymentSuccessPage;