import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Container, Row, Col, Spinner } from 'react-bootstrap';
import { FiTrendingUp } from 'react-icons/fi';
import ProductCard from './ProductCard.jsx';
import {
  fetchBestsellersThunk,
  selectBestsellers,
  selectBestsellersLoading,
} from '../../store/slices/productSlice.js';

const BestsellerSection = () => {
  const dispatch = useDispatch();
  const bestsellers = useSelector(selectBestsellers);
  const loading = useSelector(selectBestsellersLoading);

  useEffect(() => {
    if (bestsellers.length === 0) {
      dispatch(fetchBestsellersThunk(8));
    }
  }, [dispatch, bestsellers.length]);

  if (loading) {
    return (
      <div className="text-center py-4">
        <Spinner animation="border" variant="primary" />
      </div>
    );
  }

  if (!bestsellers.length) return null;

  return (
    <section className="py-4">
      <Container fluid="xl">
        <div className="d-flex align-items-center gap-2 mb-3">
          <FiTrendingUp size={22} className="text-danger" />
          <h4 className="fw-bold mb-0">Bestsellers</h4>
        </div>
        <Row xs={1} sm={2} md={3} lg={4} className="g-3">
          {bestsellers.map((product) => (
            <Col key={product.id}>
              <ProductCard product={product} />
            </Col>
          ))}
        </Row>
      </Container>
    </section>
  );
};

export default BestsellerSection;