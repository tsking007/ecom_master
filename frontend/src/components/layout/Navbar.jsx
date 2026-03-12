import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, Link } from 'react-router-dom';
import {
  Navbar as BsNavbar,
  Container,
  Nav,
  Button,
  Dropdown,
} from 'react-bootstrap';
import { FiUser, FiLogOut, FiLogIn } from 'react-icons/fi';
import { toast } from 'react-toastify';
import SearchBar from '../common/SearchBar.jsx';
import CartIcon from '../cart/CartIcon.jsx';
import { logoutThunk } from '../../store/slices/authSlice.js';
import {
  selectIsAuthenticated,
  selectCurrentUser,
} from '../../store/slices/authSlice.js';
import { resetCart } from '../../store/slices/cartSlice.js';

const AppNavbar = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const user = useSelector(selectCurrentUser);

  const handleLogout = async () => {
    await dispatch(logoutThunk());
    dispatch(resetCart());
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
                    My Cart
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