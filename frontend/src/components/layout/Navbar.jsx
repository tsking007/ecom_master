import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, Link } from 'react-router-dom';
import {
  Navbar as BsNavbar,
  Container,
  Nav,
  Button,
  Dropdown,
  Badge,
} from 'react-bootstrap';
import { FiUser, FiLogOut, FiLogIn, FiHeart, FiPackage, FiMapPin } from 'react-icons/fi';
import { toast } from 'react-toastify';
import SearchBar from '../common/SearchBar.jsx';
import CartIcon from '../cart/CartIcon.jsx';
import { logoutThunk, selectIsAuthenticated, selectCurrentUser } from '../../store/slices/authSlice.js';
import { resetCart } from '../../store/slices/cartSlice.js';
import { resetWishlist, selectWishlistTotalItems } from '../../store/slices/wishlistSlice.js';
import { resetOrders } from '../../store/slices/orderSlice.js';

const AppNavbar = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const user = useSelector(selectCurrentUser);
  const wishlistCount = useSelector(selectWishlistTotalItems);

  const handleLogout = async () => {
    await dispatch(logoutThunk());
    dispatch(resetCart());
    dispatch(resetWishlist());
    dispatch(resetOrders());
    toast.success('Logged out successfully');
    navigate('/');
  };

  return (
    <BsNavbar bg="dark" variant="dark" expand="lg" sticky="top" className="shadow-sm">
      <Container fluid="xl">
        {/* Brand */}
        <BsNavbar.Brand as={Link} to="/" className="fw-bold fs-4 me-4">
          🛒 ShopMaster
        </BsNavbar.Brand>

        <BsNavbar.Toggle aria-controls="main-navbar" />

        <BsNavbar.Collapse id="main-navbar">
          {/* Search — takes up remaining space */}
          <div className="flex-grow-1 my-2 my-lg-0 me-lg-3">
            <SearchBar />
          </div>

          <Nav className="align-items-center gap-2 ms-auto">
            {/* Wishlist icon — only when authenticated */}
            {isAuthenticated && (
              <Nav.Link
                as={Link}
                to="/wishlist"
                className="position-relative text-white p-1"
                title="Wishlist"
              >
                <FiHeart size={20} />
                {wishlistCount > 0 && (
                  <Badge
                    bg="danger"
                    pill
                    className="position-absolute"
                    style={{ top: '-4px', right: '-6px', fontSize: '0.6rem', minWidth: '16px' }}
                  >
                    {wishlistCount > 99 ? '99+' : wishlistCount}
                  </Badge>
                )}
              </Nav.Link>
            )}

            <CartIcon />

            {isAuthenticated ? (
              <Dropdown align="end">
                <Dropdown.Toggle
                  variant="outline-light"
                  id="user-menu"
                  className="d-flex align-items-center gap-2"
                >
                  <FiUser />
                  <span className="d-none d-lg-inline">
                    {user?.firstName || user?.email || 'Account'}
                  </span>
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.Item as={Link} to="/cart">
                    <FiUser className="me-2" /> My Cart
                  </Dropdown.Item>
                  <Dropdown.Item as={Link} to="/wishlist">
                    <FiHeart className="me-2" /> My Wishlist
                  </Dropdown.Item>
                  <Dropdown.Item as={Link} to="/profile?tab=orders">
                    <FiPackage className="me-2" /> My Orders
                  </Dropdown.Item>
                  <Dropdown.Item as={Link} to="/profile?tab=addresses">
                    <FiMapPin className="me-2" /> My Addresses
                  </Dropdown.Item>
                  <Dropdown.Divider />
                  <Dropdown.Item onClick={handleLogout} className="text-danger">
                    <FiLogOut className="me-2" />
                    Logout
                  </Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            ) : (
              <Button
                variant="outline-light"
                size="sm"
                onClick={() => navigate('/auth/login')}
                className="d-flex align-items-center gap-2"
              >
                <FiLogIn />
                <span>Login</span>
              </Button>
            )}
          </Nav>
        </BsNavbar.Collapse>
      </Container>
    </BsNavbar>
  );
};

export default AppNavbar;