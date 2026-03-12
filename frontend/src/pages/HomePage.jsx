import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Container, Row, Col, Button, Spinner } from 'react-bootstrap';
import { FiArrowRight } from 'react-icons/fi';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import CategorySection from '../components/product/CategorySection.jsx';
import BestsellerSection from '../components/product/BestsellerSection.jsx';
import ProductGrid from '../components/product/ProductGrid.jsx';
import {
  fetchProductsThunk,
  fetchCategoriesThunk,
  selectProducts,
  selectListLoading,
  selectListError,
} from '../store/slices/productSlice.js';

const HomePage = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const products = useSelector(selectProducts);
  const loading = useSelector(selectListLoading);
  const error = useSelector(selectListError);

  useEffect(() => {
    dispatch(fetchCategoriesThunk());
    dispatch(fetchProductsThunk({ pageNumber: 1, pageSize: 8, sortBy: 'createdAt', sortDescending: true }));
  }, [dispatch]);

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      {/* Hero */}
      <section
        className="text-white text-center py-5"
        style={{
          background: 'linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%)',
          minHeight: '280px',
          display: 'flex',
          alignItems: 'center',
        }}
      >
        <Container>
          <h1 className="display-5 fw-bold mb-3">Find Everything You Need</h1>
          <p className="lead text-white-50 mb-4">
            Thousands of products. Unbeatable prices. Fast delivery.
          </p>
          <Button
            variant="primary"
            size="lg"
            onClick={() => navigate('/products')}
            className="px-5"
          >
            Shop Now <FiArrowRight className="ms-2" />
          </Button>
        </Container>
      </section>

      {/* Categories */}
      <CategorySection />

      {/* Bestsellers */}
      <BestsellerSection />

      {/* Featured / New Arrivals */}
      <section className="py-4">
        <Container fluid="xl">
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h4 className="fw-bold mb-0">New Arrivals</h4>
            <Button
              variant="outline-primary"
              size="sm"
              onClick={() => navigate('/products')}
              className="d-flex align-items-center gap-1"
            >
              View All <FiArrowRight />
            </Button>
          </div>
          <ProductGrid
            products={products}
            loading={loading}
            error={error}
            emptyMessage="No products available right now."
          />
        </Container>
      </section>

      <Footer />
    </div>
  );
};

export default HomePage;