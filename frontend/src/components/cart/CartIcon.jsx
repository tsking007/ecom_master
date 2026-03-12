import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Button, Badge } from 'react-bootstrap';
import { FiShoppingCart } from 'react-icons/fi';
import { selectCartTotalItems } from '../../store/slices/cartSlice.js';
import { selectIsAuthenticated } from '../../store/slices/authSlice.js';

const CartIcon = () => {
  const navigate = useNavigate();
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const totalItems = useSelector(selectCartTotalItems);

  if (!isAuthenticated) return null;

  return (
    <Button
      variant="outline-light"
      className="position-relative"
      onClick={() => navigate('/cart')}
      aria-label={`Cart, ${totalItems} items`}
    >
      <FiShoppingCart size={20} />
      {totalItems > 0 && (
        <Badge
          bg="danger"
          pill
          className="position-absolute top-0 start-100 translate-middle"
          style={{ fontSize: '0.65rem' }}
        >
          {totalItems > 99 ? '99+' : totalItems}
        </Badge>
      )}
    </Button>
  );
};

export default CartIcon;