import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import {
  Container, Row, Col, Button, Badge, Spinner, Alert,
  Image, ButtonGroup, Card,
} from 'react-bootstrap';
import { FiShoppingCart, FiArrowLeft, FiMinus, FiPlus } from 'react-icons/fi';
import { toast } from 'react-toastify';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import StarRating from '../components/common/StarRating.jsx';
import {
  fetchProductBySlugThunk,
  clearSelectedProduct,
  selectSelectedProduct,
  selectDetailLoading,
  selectDetailError,
} from '../store/slices/productSlice.js';
import { addToCartThunk, selectCartMutating } from '../store/slices/cartSlice.js';
import { selectIsAuthenticated } from '../store/slices/authSlice.js';

const PLACEHOLDER = 'https://placehold.co/600x500?text=No+Image';

const ProductDetailPage = () => {
  const { slug } = useParams();
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const product = useSelector(selectSelectedProduct);
  const loading = useSelector(selectDetailLoading);
  const error = useSelector(selectDetailError);
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const mutating = useSelector(selectCartMutating);

  const [quantity, setQuantity] = useState(1);
  const [selectedImage, setSelectedImage] = useState(null);

  useEffect(() => {
    dispatch(fetchProductBySlugThunk(slug));
    return () => dispatch(clearSelectedProduct());
  }, [dispatch, slug]);

  // Set first image as default when product loads
  useEffect(() => {
    if (product) {
      setSelectedImage(product.mainImageUrl || product.imageUrls?.[0] || null);
      setQuantity(1);
    }
  }, [product]);

  const handleAddToCart = async () => {
    if (!isAuthenticated) {
      navigate('/auth/login');
      return;
    }
    const result = await dispatch(addToCartThunk({ productId: product.id, quantity }));
    if (addToCartThunk.fulfilled.match(result)) {
      toast.success(`${quantity}× "${product.name}" added to cart!`);
    } else {
      toast.error(result.payload || 'Could not add to cart');
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

  if (error || !product) {
    return (
      <>
        <AppNavbar />
        <Container className="py-5 text-center">
          <Alert variant="danger" className="d-inline-block">
            {error || 'Product not found.'}
          </Alert>
          <div className="mt-3">
            <Button variant="outline-primary" onClick={() => navigate(-1)}>
              <FiArrowLeft className="me-2" />
              Go Back
            </Button>
          </div>
        </Container>
        <Footer />
      </>
    );
  }

  const {
    name, brand, description, shortDescription,
    price, effectivePrice, discountPercentage,
    averageRating, reviewCount, availableStock,
    category, subCategory, tags, imageUrls,
    weight, dimensions, soldCount, isFeatured,
  } = product;

  const allImages = imageUrls?.length ? imageUrls : [PLACEHOLDER];

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container fluid="xl" className="py-4 flex-grow-1">
        {/* Breadcrumb */}
        <nav aria-label="breadcrumb" className="mb-3">
          <ol className="breadcrumb small">
            <li className="breadcrumb-item">
              <span
                className="text-primary"
                style={{ cursor: 'pointer' }}
                onClick={() => navigate('/')}
              >
                Home
              </span>
            </li>
            <li className="breadcrumb-item">
              <span
                className="text-primary"
                style={{ cursor: 'pointer' }}
                onClick={() => navigate(`/products?category=${encodeURIComponent(category)}`)}
              >
                {category}
              </span>
            </li>
            {subCategory && (
              <li className="breadcrumb-item text-muted">{subCategory}</li>
            )}
            <li className="breadcrumb-item active text-truncate" style={{ maxWidth: '200px' }}>
              {name}
            </li>
          </ol>
        </nav>

        <Row className="g-4">
          {/* ── Left: Images ─────────────────────────────────────── */}
          <Col md={5}>
            <div
              className="border rounded overflow-hidden mb-2"
              style={{ background: '#f8f9fa', height: '400px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
            >
              <Image
                src={selectedImage || PLACEHOLDER}
                alt={name}
                style={{ maxHeight: '100%', maxWidth: '100%', objectFit: 'contain' }}
                onError={(e) => { e.target.src = PLACEHOLDER; }}
              />
            </div>
            {/* Thumbnail strip */}
            {allImages.length > 1 && (
              <div className="d-flex gap-2 flex-wrap">
                {allImages.map((url, i) => (
                  <div
                    key={i}
                    onClick={() => setSelectedImage(url)}
                    style={{
                      width: '60px',
                      height: '60px',
                      cursor: 'pointer',
                      border: selectedImage === url ? '2px solid #0d6efd' : '1px solid #dee2e6',
                      borderRadius: '6px',
                      overflow: 'hidden',
                      flexShrink: 0,
                    }}
                  >
                    <img
                      src={url}
                      alt={`${name} ${i + 1}`}
                      style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                      onError={(e) => { e.target.src = PLACEHOLDER; }}
                    />
                  </div>
                ))}
              </div>
            )}
          </Col>

          {/* ── Right: Details ───────────────────────────────────── */}
          <Col md={7}>
            {/* Brand + badges */}
            <div className="d-flex align-items-center gap-2 mb-1 flex-wrap">
              {brand && (
                <span className="text-muted small text-uppercase fw-semibold">{brand}</span>
              )}
              {isFeatured && <Badge bg="warning" text="dark">Featured</Badge>}
              {availableStock === 0 && <Badge bg="danger">Out of Stock</Badge>}
              {availableStock > 0 && availableStock <= 5 && (
                <Badge bg="warning" text="dark">Only {availableStock} left!</Badge>
              )}
            </div>

            <h2 className="fw-bold mb-2">{name}</h2>

            <div className="mb-3">
              <StarRating rating={averageRating} reviewCount={reviewCount} size="md" />
              <small className="text-muted ms-2">{soldCount} sold</small>
            </div>

            {/* Price */}
            <div className="d-flex align-items-baseline gap-3 mb-3">
              <span className="fw-bold text-primary" style={{ fontSize: '1.8rem' }}>
                ₹{effectivePrice?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
              </span>
              {discountPercentage > 0 && (
                <>
                  <span className="text-muted text-decoration-line-through fs-5">
                    ₹{price?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                  </span>
                  <Badge bg="success" style={{ fontSize: '0.85rem' }}>
                    {Math.round(discountPercentage)}% OFF
                  </Badge>
                </>
              )}
            </div>

            {shortDescription && (
              <p className="text-muted mb-3">{shortDescription}</p>
            )}

            {/* Quantity selector + CTA */}
            {availableStock > 0 ? (
              <div className="d-flex align-items-center gap-3 mb-4 flex-wrap">
                <div>
                  <small className="text-muted d-block mb-1">Quantity</small>
                  <ButtonGroup>
                    <Button
                      variant="outline-secondary"
                      size="sm"
                      onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                      disabled={quantity <= 1}
                    >
                      <FiMinus />
                    </Button>
                    <Button variant="outline-secondary" size="sm" disabled style={{ minWidth: '42px' }}>
                      {quantity}
                    </Button>
                    <Button
                      variant="outline-secondary"
                      size="sm"
                      onClick={() => setQuantity((q) => Math.min(availableStock, q + 1))}
                      disabled={quantity >= availableStock}
                    >
                      <FiPlus />
                    </Button>
                  </ButtonGroup>
                </div>

                {isAuthenticated ? (
                  <Button
                    variant="primary"
                    size="lg"
                    onClick={handleAddToCart}
                    disabled={mutating}
                    className="d-flex align-items-center gap-2"
                  >
                    {mutating
                      ? <Spinner animation="border" size="sm" />
                      : <FiShoppingCart />
                    }
                    Add to Cart
                  </Button>
                ) : (
                  <Button
                    variant="outline-primary"
                    size="lg"
                    onClick={() => navigate('/auth/login')}
                    className="d-flex align-items-center gap-2"
                  >
                    <FiShoppingCart />
                    Login to Buy
                  </Button>
                )}
              </div>
            ) : (
              <Button variant="secondary" size="lg" disabled className="mb-4">
                Out of Stock
              </Button>
            )}

            {/* Meta info */}
            <Card className="border-0 bg-light p-3 mb-3">
              <Row className="g-2 small text-muted">
                {category && (
                  <Col xs={6}>
                    <strong>Category:</strong> {category}
                    {subCategory && ` › ${subCategory}`}
                  </Col>
                )}
                {weight && (
                  <Col xs={6}><strong>Weight:</strong> {weight} kg</Col>
                )}
                {dimensions && (
                  <Col xs={6}><strong>Dimensions:</strong> {dimensions}</Col>
                )}
                <Col xs={6}>
                  <strong>Availability:</strong>{' '}
                  {availableStock > 0 ? `${availableStock} in stock` : 'Out of stock'}
                </Col>
              </Row>
            </Card>

            {/* Tags */}
            {tags?.length > 0 && (
              <div className="d-flex flex-wrap gap-1">
                {tags.map((tag) => (
                  <Badge key={tag} bg="secondary" pill style={{ fontSize: '0.75rem' }}>
                    {tag}
                  </Badge>
                ))}
              </div>
            )}
          </Col>
        </Row>

        {/* Full description */}
        {description && (
          <Row className="mt-4">
            <Col>
              <Card className="border-0 shadow-sm">
                <Card.Header className="bg-white fw-bold">Product Description</Card.Header>
                <Card.Body>
                  <p className="mb-0" style={{ whiteSpace: 'pre-line', lineHeight: '1.7' }}>
                    {description}
                  </p>
                </Card.Body>
              </Card>
            </Col>
          </Row>
        )}
      </Container>

      <Footer />
    </div>
  );
};

export default ProductDetailPage;