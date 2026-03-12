import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Spinner, Badge } from 'react-bootstrap';
import { FiGrid } from 'react-icons/fi';
import {
  fetchCategoriesThunk,
  selectCategories,
  setActiveFilters,
} from '../../store/slices/productSlice.js';

// Simple colour palette for category cards
const CATEGORY_COLORS = [
  '#4361ee', '#3a0ca3', '#7209b7', '#f72585',
  '#4cc9f0', '#06d6a0', '#fb8500', '#e63946',
];

const CategorySection = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const categories = useSelector(selectCategories);

  useEffect(() => {
    if (categories.length === 0) {
      dispatch(fetchCategoriesThunk());
    }
  }, [dispatch, categories.length]);

  const handleCategoryClick = (categoryName) => {
    dispatch(setActiveFilters({ category: categoryName }));
    navigate(`/products?category=${encodeURIComponent(categoryName)}`);
  };

  if (!categories.length) return null;

  return (
    <section className="py-4 bg-light">
      <Container fluid="xl">
        <div className="d-flex align-items-center gap-2 mb-3">
          <FiGrid size={22} className="text-primary" />
          <h4 className="fw-bold mb-0">Shop by Category</h4>
        </div>
        <Row xs={2} sm={3} md={4} lg={6} className="g-3">
          {categories.map((cat, idx) => (
            <Col key={cat.category}>
              <Card
                className="text-center border-0 shadow-sm h-100"
                style={{ cursor: 'pointer', borderRadius: '12px', overflow: 'hidden' }}
                onClick={() => handleCategoryClick(cat.category)}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-3px)';
                  e.currentTarget.style.boxShadow = '0 6px 18px rgba(0,0,0,0.15)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = '';
                  e.currentTarget.style.boxShadow = '';
                }}
              >
                <div
                  style={{
                    background: CATEGORY_COLORS[idx % CATEGORY_COLORS.length],
                    height: '70px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <span style={{ fontSize: '2rem' }}>🏷️</span>
                </div>
                <Card.Body className="py-2 px-1">
                  <p className="mb-0 fw-semibold small">{cat.category}</p>
                  <Badge bg="secondary" pill style={{ fontSize: '0.65rem' }}>
                    {cat.count}
                  </Badge>
                </Card.Body>
              </Card>
            </Col>
          ))}
        </Row>
      </Container>
    </section>
  );
};

export default CategorySection;