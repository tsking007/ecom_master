import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Container, Button, Spinner } from 'react-bootstrap';
import { FiXCircle, FiShoppingCart, FiRefreshCw } from 'react-icons/fi';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import { getSessionStatusApi } from '../api/paymentsApi.js';

const PaymentFailurePage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const sessionId = searchParams.get('session_id');

  const [orderNumber, setOrderNumber] = useState(null);
  const [loading, setLoading] = useState(!!sessionId);

  useEffect(() => {
    if (!sessionId) return;

    const fetchStatus = async () => {
      try {
        const res = await getSessionStatusApi(sessionId);
        setOrderNumber(res.data.orderNumber);
      } catch {
        // Silently ignore — we still show the failure UI
      } finally {
        setLoading(false);
      }
    };

    fetchStatus();
  }, [sessionId]);

  if (loading) {
    return (
      <>
        <AppNavbar />
        <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '60vh' }}>
          <Spinner animation="border" variant="danger" />
        </div>
        <Footer />
      </>
    );
  }

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container className="py-5 text-center flex-grow-1" style={{ maxWidth: '480px' }}>
        <FiXCircle size={64} className="text-danger mb-3" />
        <h3 className="fw-bold text-danger mb-2">Payment Failed</h3>
        {orderNumber && (
          <p className="text-muted mb-1">
            Order <strong>#{orderNumber}</strong> could not be completed.
          </p>
        )}
        <p className="text-muted mb-4">
          Your payment was not successful. Your cart items are still saved — you can try again.
        </p>

        <div className="d-flex flex-column gap-3 align-items-center">
          <Button
            variant="primary"
            size="lg"
            onClick={() => navigate('/checkout')}
            className="d-flex align-items-center gap-2 px-4"
          >
            <FiRefreshCw /> Try Again
          </Button>
          <Button
            variant="outline-secondary"
            onClick={() => navigate('/cart')}
            className="d-flex align-items-center gap-2"
          >
            <FiShoppingCart /> Back to Cart
          </Button>
          <Button
            variant="link"
            className="text-muted"
            onClick={() => navigate('/profile?tab=orders')}
          >
            View My Orders
          </Button>
        </div>
      </Container>

      <Footer />
    </div>
  );
};

export default PaymentFailurePage;