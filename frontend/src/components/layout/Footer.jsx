import { Container, Row, Col } from 'react-bootstrap';
import { Link } from 'react-router-dom';

const Footer = () => (
  <footer className="bg-dark text-light py-4 mt-auto">
    <Container>
      <Row className="gy-3">
        <Col md={4}>
          <h6 className="fw-bold">🛒 ShopMaster</h6>
          <p className="text-muted small mb-0">
            Your one-stop shop for everything.
          </p>
        </Col>
        <Col md={4}>
          <h6 className="fw-bold">Quick Links</h6>
          <ul className="list-unstyled small mb-0">
            <li><Link to="/" className="text-muted text-decoration-none">Home</Link></li>
            <li><Link to="/products" className="text-muted text-decoration-none">All Products</Link></li>
            <li><Link to="/cart" className="text-muted text-decoration-none">Cart</Link></li>
          </ul>
        </Col>
        <Col md={4}>
          <h6 className="fw-bold">Account</h6>
          <ul className="list-unstyled small mb-0">
            <li><Link to="/auth/login" className="text-muted text-decoration-none">Login</Link></li>
            <li><Link to="/auth/signup" className="text-muted text-decoration-none">Sign Up</Link></li>
          </ul>
        </Col>
      </Row>
      <hr className="border-secondary mt-3 mb-2" />
      <p className="text-center text-muted small mb-0">
        © {new Date().getFullYear()} ShopMaster. All rights reserved.
      </p>
    </Container>
  </footer>
);

export default Footer;