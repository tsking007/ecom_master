import { Row, Col, Spinner, Alert } from 'react-bootstrap';
import ProductCard from './ProductCard.jsx';

const ProductGrid = ({ products = [], loading = false, error = null, emptyMessage = 'No products found.' }) => {
  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center py-5">
        <Spinner animation="border" variant="primary" role="status">
          <span className="visually-hidden">Loading products...</span>
        </Spinner>
      </div>
    );
  }

  if (error) {
    return <Alert variant="danger">{error}</Alert>;
  }

  if (!products.length) {
    return (
      <div className="text-center py-5 text-muted">
        <p className="fs-5">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <Row xs={1} sm={2} md={3} lg={4} className="g-3">
      {products.map((product) => (
        <Col key={product.id}>
          <ProductCard product={product} />
        </Col>
      ))}
    </Row>
  );
};

export default ProductGrid;