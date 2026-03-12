import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, Link } from 'react-router-dom';
import {
  Container, Row, Col, Card, Button, Badge,
  Spinner, Alert, Image, Table,
} from 'react-bootstrap';
import { FiTrash2, FiMinus, FiPlus, FiShoppingCart, FiArrowRight } from 'react-icons/fi';
import { toast } from 'react-toastify';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import {
  fetchCartThunk,
  updateCartItemThunk,
  removeCartItemThunk,
  clearCartThunk,
  selectCartItems,
  selectCartSubtotal,
  selectCartTotalItems,
  selectCartLoading,
  selectCartMutating,
  selectHasOutOfStockItems,
  selectHasPriceChanges,
} from '../store/slices/cartSlice.js';
import { selectIsAuthenticated, selectCurrentUser } from '../store/slices/authSlice.js';

const PLACEHOLDER = 'https://placehold.co/80x80?text=?';

const CartPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const isAuthenticated = useSelector(selectIsAuthenticated);
  const user = useSelector(selectCurrentUser);
  const items = useSelector(selectCartItems);
  const subtotal = useSelector(selectCartSubtotal);
  const totalItems = useSelector(selectCartTotalItems);
  const loading = useSelector(selectCartLoading);
  const mutating = useSelector(selectCartMutating);
  const hasOutOfStock = useSelector(selectHasOutOfStockItems);
  const hasPriceChanges = useSelector(selectHasPriceChanges);

  useEffect(() => {
    if (isAuthenticated) {
      dispatch(fetchCartThunk());
    }
  }, [dispatch, isAuthenticated]);

  // Guard: not logged in
  if (!isAuthenticated) {
    return (
      <>
        <AppNavbar />
        <Container className="py-5 text-center">
          <FiShoppingCart size={64} className="text-muted mb-3" />
          <h4>Your cart is waiting</h4>
          <p className="text-muted">Please log in to view your cart.</p>
          <Button variant="primary" onClick={() => navigate('/auth/login')}>
            Login to Continue
          </Button>
        </Container>
        <Footer />
      </>
    );
  }

  if (loading) {
    return (
      <>
        <AppNavbar />
        <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '60vh' }}>
          <Spinner animation="border" variant="primary" />
        </div>
        <Footer />
      </>
    );
  }

  const handleQuantityChange = async (cartItemId, newQty) => {
    if (newQty < 1) return;
    const result = await dispatch(updateCartItemThunk({ cartItemId, quantity: newQty }));
    if (updateCartItemThunk.rejected.match(result)) {
      toast.error(result.payload || 'Could not update quantity');
    }
  };

  const handleRemove = async (cartItemId, productName) => {
    const result = await dispatch(removeCartItemThunk(cartItemId));
    if (removeCartItemThunk.fulfilled.match(result)) {
      toast.info(`"${productName}" removed from cart`);
    } else {
      toast.error('Could not remove item');
    }
  };

  const handleClearCart = async () => {
    if (!window.confirm('Clear all items from your cart?')) return;
    await dispatch(clearCartThunk());
    toast.info('Cart cleared');
  };

  const handleCheckout = () => {
    if (hasOutOfStock) {
      toast.warning('Please remove out-of-stock items before checking out.');
      return;
    }
    navigate('/checkout');
  };

  // Empty cart
  if (!items.length) {
    return (
      <>
        <AppNavbar />
        <Container className="py-5 text-center">
          <FiShoppingCart size={64} className="text-muted mb-3" />
          <h4>Your cart is empty</h4>
          <p className="text-muted">Add some products to get started.</p>
          <Button variant="primary" onClick={() => navigate('/products')}>
            Browse Products
          </Button>
        </Container>
        <Footer />
      </>
    );
  }

  const deliveryAddress = user?.address;

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container fluid="xl" className="py-4 flex-grow-1">
        <div className="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
          <h4 className="fw-bold mb-0">
            Shopping Cart{' '}
            <Badge bg="secondary" pill>{totalItems} item{totalItems !== 1 ? 's' : ''}</Badge>
          </h4>
          <Button
            variant="outline-danger"
            size="sm"
            onClick={handleClearCart}
            disabled={mutating}
          >
            <FiTrash2 className="me-1" /> Clear Cart
          </Button>
        </div>

        {/* Alerts */}
        {hasOutOfStock && (
          <Alert variant="warning" className="py-2">
            ⚠️ Some items are out of stock. Please remove them before checkout.
          </Alert>
        )}
        {hasPriceChanges && (
          <Alert variant="info" className="py-2">
            ℹ️ Prices for some items have changed since you added them.
          </Alert>
        )}

        <Row className="g-4">
          {/* ── Cart Items ──────────────────────────────────────── */}
          <Col lg={8}>
            <Card className="border-0 shadow-sm">
              <Card.Body className="p-0">
                {items.map((item, idx) => (
                  <div
                    key={item.cartItemId}
                    className={`p-3 ${idx < items.length - 1 ? 'border-bottom' : ''}`}
                  >
                    <Row className="align-items-center g-3">
                      {/* Image */}
                      <Col xs={3} sm={2}>
                        <Link to={`/products/${item.productSlug}`}>
                          <Image
                            src={item.mainImageUrl || PLACEHOLDER}
                            alt={item.productName}
                            rounded
                            style={{ width: '70px', height: '70px', objectFit: 'cover' }}
                            onError={(e) => { e.target.src = PLACEHOLDER; }}
                          />
                        </Link>
                      </Col>

                      {/* Name + badges */}
                      <Col xs={9} sm={5}>
                        <Link
                          to={`/products/${item.productSlug}`}
                          className="text-decoration-none text-dark fw-semibold"
                        >
                          {item.productName}
                        </Link>
                        <div className="mt-1 d-flex flex-wrap gap-1">
                          {item.isOutOfStock && (
                            <Badge bg="danger" pill style={{ fontSize: '0.7rem' }}>Out of Stock</Badge>
                          )}
                          {item.hasPriceChanged && !item.isOutOfStock && (
                            <Badge bg="warning" text="dark" pill style={{ fontSize: '0.7rem' }}>
                              Price changed
                            </Badge>
                          )}
                          {!item.isActiveProduct && (
                            <Badge bg="secondary" pill style={{ fontSize: '0.7rem' }}>Unavailable</Badge>
                          )}
                        </div>
                        {item.hasPriceChanged && (
                          <small className="text-muted d-block">
                            Was ₹{item.priceAtAddition?.toLocaleString('en-IN')}
                          </small>
                        )}
                      </Col>

                      {/* Qty controls */}
                      <Col xs={6} sm={3}>
                        <div className="d-flex align-items-center gap-1">
                          <Button
                            variant="outline-secondary"
                            size="sm"
                            onClick={() => handleQuantityChange(item.cartItemId, item.quantity - 1)}
                            disabled={mutating || item.quantity <= 1}
                            style={{ width: '30px', height: '30px', padding: 0 }}
                          >
                            <FiMinus size={12} />
                          </Button>
                          <span
                            className="text-center fw-bold"
                            style={{ minWidth: '28px', fontSize: '0.9rem' }}
                          >
                            {item.quantity}
                          </span>
                          <Button
                            variant="outline-secondary"
                            size="sm"
                            onClick={() => handleQuantityChange(item.cartItemId, item.quantity + 1)}
                            disabled={mutating || item.quantity >= item.availableStock}
                            style={{ width: '30px', height: '30px', padding: 0 }}
                          >
                            <FiPlus size={12} />
                          </Button>
                        </div>
                        <small className="text-muted d-block mt-1" style={{ fontSize: '0.7rem' }}>
                          {item.availableStock} available
                        </small>
                      </Col>

                      {/* Line total + remove */}
                      <Col xs={6} sm={2} className="text-end">
                        <div className="fw-bold text-primary">
                          ₹{item.lineTotal?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </div>
                        <small className="text-muted d-block" style={{ fontSize: '0.75rem' }}>
                          ₹{item.currentUnitPrice?.toLocaleString('en-IN')} each
                        </small>
                        <Button
                          variant="link"
                          size="sm"
                          className="text-danger p-0 mt-1"
                          onClick={() => handleRemove(item.cartItemId, item.productName)}
                          disabled={mutating}
                        >
                          <FiTrash2 size={14} />
                        </Button>
                      </Col>
                    </Row>
                  </div>
                ))}
              </Card.Body>
            </Card>
          </Col>

          {/* ── Order Summary ────────────────────────────────────── */}
          <Col lg={4}>
            <Card className="border-0 shadow-sm mb-3">
              <Card.Header className="bg-white fw-bold">Order Summary</Card.Header>
              <Card.Body>
                <Table borderless size="sm" className="mb-0">
                  <tbody>
                    {items.map((item) => (
                      <tr key={item.cartItemId}>
                        <td className="text-muted small" style={{ maxWidth: '160px' }}>
                          <span
                            className="d-inline-block text-truncate"
                            style={{ maxWidth: '140px' }}
                          >
                            {item.productName}
                          </span>
                          <span className="ms-1 text-muted">×{item.quantity}</span>
                        </td>
                        <td className="text-end small fw-semibold">
                          ₹{item.lineTotal?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    ))}
                    <tr>
                      <td colSpan={2}><hr className="my-2" /></td>
                    </tr>
                    <tr>
                      <td className="fw-bold">Subtotal</td>
                      <td className="text-end fw-bold text-primary fs-5">
                        ₹{subtotal?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                      </td>
                    </tr>
                    <tr>
                      <td className="text-muted small">Delivery</td>
                      <td className="text-end text-success small">Calculated at checkout</td>
                    </tr>
                  </tbody>
                </Table>
              </Card.Body>
            </Card>

            {/* Delivery Address */}
            <Card className="border-0 shadow-sm mb-3">
              <Card.Header className="bg-white fw-bold">Delivery Address</Card.Header>
              <Card.Body>
                {deliveryAddress ? (
                  <address className="mb-0 small">
                    <strong>{user?.firstName} {user?.lastName}</strong><br />
                    {deliveryAddress.street && <>{deliveryAddress.street}<br /></>}
                    {deliveryAddress.city && <>{deliveryAddress.city}, </>}
                    {deliveryAddress.state && <>{deliveryAddress.state}<br /></>}
                    {deliveryAddress.postalCode && <>{deliveryAddress.postalCode}<br /></>}
                    {deliveryAddress.country}
                  </address>
                ) : (
                  <div className="text-muted small">
                    <p className="mb-1">No delivery address saved.</p>
                    <p className="mb-0">You can add your address during checkout.</p>
                  </div>
                )}
              </Card.Body>
            </Card>

            {/* Checkout button */}
            <div className="d-grid">
              <Button
                variant="success"
                size="lg"
                onClick={handleCheckout}
                disabled={mutating || hasOutOfStock || !items.length}
                className="d-flex align-items-center justify-content-center gap-2"
              >
                {mutating
                  ? <Spinner animation="border" size="sm" />
                  : <>Proceed to Checkout <FiArrowRight /></>
                }
              </Button>
              {hasOutOfStock && (
                <small className="text-danger text-center mt-1">
                  Remove out-of-stock items to continue
                </small>
              )}
            </div>
          </Col>
        </Row>
      </Container>

      <Footer />
    </div>
  );
};

export default CartPage;