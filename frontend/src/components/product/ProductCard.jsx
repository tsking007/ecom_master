import { Card, Badge, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { toast } from 'react-toastify';
import { FiShoppingCart } from 'react-icons/fi';
import StarRating from '../common/StarRating.jsx';
import { addToCartThunk, selectCartMutating } from '../../store/slices/cartSlice.js';
import { selectIsAuthenticated } from '../../store/slices/authSlice.js';

const PLACEHOLDER = 'https://placehold.co/400x300?text=No+Image';

const ProductCard = ({ product }) => {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const mutating = useSelector(selectCartMutating);

  const {
    slug,
    name,
    mainImageUrl,
    effectivePrice,
    price,
    discountPercentage,
    averageRating,
    reviewCount,
    availableStock,
    brand,
  } = product;

  const handleCardClick = () => navigate(`/products/${slug}`);

  const handleAddToCart = async (e) => {
    e.stopPropagation(); // don't navigate on button click
    if (!isAuthenticated) {
      navigate('/auth/login');
      return;
    }
    const result = await dispatch(addToCartThunk({ productId: product.id, quantity: 1 }));
    if (addToCartThunk.fulfilled.match(result)) {
      toast.success(`"${name}" added to cart!`);
    } else {
      toast.error(result.payload || 'Could not add to cart');
    }
  };

  return (
    <Card
      className="h-100 shadow-sm product-card"
      onClick={handleCardClick}
      style={{ cursor: 'pointer', transition: 'transform 0.15s, box-shadow 0.15s' }}
      onMouseEnter={(e) => {
        e.currentTarget.style.transform = 'translateY(-3px)';
        e.currentTarget.style.boxShadow = '0 6px 20px rgba(0,0,0,0.12)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.transform = 'translateY(0)';
        e.currentTarget.style.boxShadow = '';
      }}
    >
      {/* Discount badge */}
      {discountPercentage > 0 && (
        <Badge
          bg="danger"
          className="position-absolute top-0 end-0 m-2"
          style={{ zIndex: 1, fontSize: '0.75rem' }}
        >
          -{Math.round(discountPercentage)}%
        </Badge>
      )}

      <Card.Img
        variant="top"
        src={mainImageUrl || PLACEHOLDER}
        alt={name}
        style={{ height: '200px', objectFit: 'cover' }}
        onError={(e) => { e.target.src = PLACEHOLDER; }}
      />

      <Card.Body className="d-flex flex-column">
        {brand && (
          <small className="text-muted text-uppercase fw-semibold" style={{ fontSize: '0.7rem' }}>
            {brand}
          </small>
        )}

        <Card.Title
          className="mb-1 mt-1"
          style={{
            fontSize: '0.95rem',
            fontWeight: 600,
            overflow: 'hidden',
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
          }}
        >
          {name}
        </Card.Title>

        <StarRating rating={averageRating} reviewCount={reviewCount} />

        {/* Price */}
        <div className="mt-auto pt-2">
          <div className="d-flex align-items-baseline gap-2">
            <span className="fw-bold fs-5 text-primary">
              ₹{effectivePrice?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
            </span>
            {discountPercentage > 0 && (
              <span className="text-muted text-decoration-line-through small">
                ₹{price?.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
              </span>
            )}
          </div>

          {availableStock === 0 ? (
            <Button variant="secondary" size="sm" className="w-100 mt-2" disabled>
              Out of Stock
            </Button>
          ) : isAuthenticated ? (
            <Button
              variant="primary"
              size="sm"
              className="w-100 mt-2"
              onClick={handleAddToCart}
              disabled={mutating}
            >
              <FiShoppingCart className="me-1" />
              Add to Cart
            </Button>
          ) : (
            <Button
              variant="outline-primary"
              size="sm"
              className="w-100 mt-2"
              onClick={(e) => { e.stopPropagation(); navigate('/auth/login'); }}
            >
              Login to Buy
            </Button>
          )}
        </div>
      </Card.Body>
    </Card>
  );
};

export default ProductCard;