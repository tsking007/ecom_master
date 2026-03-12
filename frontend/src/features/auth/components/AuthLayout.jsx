import { Container, Row, Col, Card } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import '../styles/auth.css';

const AuthLayout = ({ title, subtitle, children, footerText, footerLinkText, footerLinkTo }) => {
  return (
    <div className="auth-wrapper">
      <Container>
        <Row className="justify-content-center align-items-center min-vh-100">
          <Col xs={12} sm={10} md={7} lg={5} xl={4}>

            {/* Brand */}
            <div className="text-center mb-4">
              <Link to="/" className="auth-brand-link">
                <span className="auth-brand-icon">🛒</span>
                <span className="auth-brand-name">EcommerceApp</span>
              </Link>
            </div>

            <Card className="auth-card shadow-sm border-0">
              <Card.Body className="p-4 p-md-5">

                {/* Title */}
                <div className="text-center mb-4">
                  <h4 className="auth-title fw-bold">{title}</h4>
                  {subtitle && (
                    <p className="auth-subtitle text-muted mt-1">{subtitle}</p>
                  )}
                </div>

                {children}

              </Card.Body>
            </Card>

            {/* Footer link */}
            {footerText && footerLinkText && footerLinkTo && (
              <p className="text-center mt-3 auth-footer-text">
                {footerText}{' '}
                <Link to={footerLinkTo} className="auth-footer-link fw-semibold">
                  {footerLinkText}
                </Link>
              </p>
            )}

          </Col>
        </Row>
      </Container>
    </div>
  );
};

export default AuthLayout;