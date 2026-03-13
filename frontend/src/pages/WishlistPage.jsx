import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, Link } from 'react-router-dom';
import {
  Container, Row, Col, Card, Button, Badge,
  Spinner, Alert, Image,
} from 'react-bootstrap';
import { FiHeart, FiTrash2, FiShoppingCart, FiArrowRight, FiTrendingDown } from 'react-icons/fi';
import { toast } from 'react-toastify';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import {
  fetchWishlistThunk,
  removeWishlistItemThunk,
  moveToCartThunk,
  selectWishlistItems,
  selectWishlistTotalItems,
  selectWishlistLoading,
  selectWishlistMutating,
  selectHasOutOfStockWishlistItems,
} from '../store/slices/wishlistSlice.js';
import { selectIsAuthenticated } from '../store/slices/authSlice.js';

const PLACEHOLDER = 'https://placehold.co/80x80?text=?';

const WishlistPage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const isAuthenticated = useSelector(selectIsAuthenticated);
  const items = useSelector(selectWishlistItems);
  const totalItems = useSelector(selectWishlistTotalItems);
  const loading = useSelector(selectWishlistLoading);
  const mutating = useSelector(selectWishlistMutating);
  const hasOutOfStock = useSelector(selectHasOutOfStockWishlistItems);

  useEffect(() => {
    if (isAuthenticated) {
      dispatch(fetchWishlistThunk());
    }
  }, [dispatch, isAuthenticated]);

  const handleRemove = async (wishlistId, productName) => {
    const result = await dispatch(removeWishlistItemThunk(wishlistId));
    if (removeWishlistItemThunk.fulfilled.match(result)) {
      toast.info(`"${productName}" removed from wishlist`);
    } else {
      toast.error('Could not remove item');
    }
  };

  const handleMoveToCart = async (wishlistId, productName) => {
    const result = await dispatch(moveToCartThunk(wishlistId));
    if (moveToCartThunk.fulfilled.match(result)) {
      toast.success(`"${productName}" moved to cart`);
    } else {
      toast.error(result.payload || 'Could not move to cart');
    }
  };

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

  if (!items.length) {
    return (
      <>
        <AppNavbar />
        <Container className="py-5 text-center">
          <FiHeart size={64} className="text-muted mb-3" />
          <h4>Your wishlist is empty</h4>
          <p className="text-muted">Save items you love and come back to them later.</p>
          <Button variant="primary" onClick={() => navigate('/products')}>
            Browse Products
          </Button>
        </Container>
        <Footer />
      </>
    );
  }

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container fluid="xl" className="py-4 flex-grow-1">
        <div className="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
          <h4 className="fw-bold mb-0">
            My Wishlist{' '}
            <Badge bg="secondary" pill>
              {totalItems} item{totalItems !== 1 ? 's' : ''}
            </Badge>
          </h4>
          <Button
            variant="outline-success"
            size="sm"
            onClick={() => navigate('/cart')}
            className="d-flex align-items-center gap-1"
          >
            <FiShoppingCart size={14} /> Go to Cart
          </Button>
        </div>

        {hasOutOfStock && (
          <Alert variant="warning" className="py-2">
            ⚠️ Some wishlist items are currently out of stock.
          </Alert>
        )}

        <Row className="g-3">
          {items.map((item) => (
            <Col key={item.wishlistId} xs={12} sm={6} lg={4} xl={3}>
              <Card className="border-0 shadow-sm h-100">
                <Card.Body className="p-3 d-flex flex-column">
                  {/* Product image + name */}
                  <div className="d-flex gap-3 mb-3">
                    <Link to={`/products/${item.productSlug}`} className="flex-shrink-0">
                      <Image
                        src={item.mainImageUrl || PLACEHOLDER}
                        alt={item.productName}
                        rounded
                        style={{ width: '72px', height: '72px', objectFit: 'cover' }}
                        onError={(e) => { e.target.src = PLACEHOLDER; }}
                      />
                    </Link>
                    <div className="flex-grow-1 min-width-0">
                      <Link
                        to={`/products/${item.productSlug}`}
                        className="text-decoration-none text-dark fw-semibold small d-block text-truncate"
                        style={{ maxWidth: '160px' }}
                      >
                        {item.productName}
                      </Link>
                      {item.brand && (
                        <div className="text-muted" style={{ fontSize: '0.72rem' }}>{item.brand}</div>
                      )}
                      <div className="text-muted" style={{ fontSize: '0.72rem' }}>{item.category}</div>

                      {/* Status badges */}
                      <div className="d-flex flex-wrap gap-1 mt-1">
                        {item.isOutOfStock && (
                          <Badge bg="danger" pill style={{ fontSize: '0.65rem' }}>Out of Stock</Badge>
                        )}
                        {!item.isActiveProduct && (
                          <Badge bg="secondary" pill style={{ fontSize: '0.65rem' }}>Unavailable</Badge>
                        )}
                        {item.hasPriceDropped && (
                          <Badge bg="success" pill style={{ fontSize: '0.65rem' }}>
                            <FiTrendingDown size={9} className="me-1" />Price Drop!
                          </Badge>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* Price */}
                  <div className="mb-3">
                    <span className="fw-bold text-primary fs-6">
                      ₹{item.currentPrice?.toLocaleString('en-IN')}
                    </span>
                    {item.hasPriceChanged && (
                      <span className="text-muted text-decoration-line-through ms-2 small">
                        ₹{item.priceAtAdd?.toLocaleString('en-IN')}
                      </span>
                    )}
                    <div className="text-muted mt-1" style={{ fontSize: '0.72rem' }}>
                      {item.availableStock > 0
                        ? `${item.availableStock} in stock`
                        : 'Out of stock'}
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="mt-auto d-flex gap-2">
                    <Button
                      variant="primary"
                      size="sm"
                      className="flex-grow-1 d-flex align-items-center justify-content-center gap-1"
                      onClick={() => handleMoveToCart(item.wishlistId, item.productName)}
                      disabled={mutating || item.isOutOfStock || !item.isActiveProduct}
                    >
                      <FiShoppingCart size={13} />
                      Move to Cart
                    </Button>
                    <Button
                      variant="outline-danger"
                      size="sm"
                      onClick={() => handleRemove(item.wishlistId, item.productName)}
                      disabled={mutating}
                      title="Remove from wishlist"
                    >
                      <FiTrash2 size={13} />
                    </Button>
                  </div>
                </Card.Body>
              </Card>
            </Col>
          ))}
        </Row>
      </Container>

      <Footer />
    </div>
  );
};

export default WishlistPage;