import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSearchParams } from 'react-router-dom';
import {
  Container, Row, Col, Offcanvas, Button,
} from 'react-bootstrap';
import { FiFilter } from 'react-icons/fi';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import ProductGrid from '../components/product/ProductGrid.jsx';
import ProductFilters from '../components/product/ProductFilters.jsx';
import Pagination from '../components/common/Pagination.jsx';
import {
  fetchProductsThunk,
  fetchCategoriesThunk,
  setActiveFilters,
  setPage,
  selectProducts,
  selectListLoading,
  selectListError,
  selectProductsPaged,
  selectActiveFilters,
} from '../store/slices/productSlice.js';

const buildApiParams = (activeFilters, pageNumber, pageSize) => {
  const params = {
    pageNumber,
    pageSize,
    // sortDescending MUST always be sent explicitly — never let it be omitted —
    // because false would be falsy and could be accidentally stripped, and
    // the backend default (true) would silently override our intended direction.
    sortDescending: activeFilters.sortDescending ?? true,
  };

  // Only append non-null / non-undefined optional filters
  const optional = [
    'category', 'subCategory', 'brand',
    'minPrice', 'maxPrice', 'minRating', 'sortBy',
  ];
  optional.forEach((key) => {
    const val = activeFilters[key];
    if (val !== null && val !== undefined) {
      params[key] = val;
    }
  });

  return params;
};

const ProductListPage = () => {
  const dispatch = useDispatch();
  const [searchParams, setSearchParams] = useSearchParams();
  const [showFilters, setShowFilters] = useState(false);

  const products     = useSelector(selectProducts);
  const loading      = useSelector(selectListLoading);
  const error        = useSelector(selectListError);
  const paged        = useSelector(selectProductsPaged);
  const activeFilters = useSelector(selectActiveFilters);

  // Sync URL query params → Redux filters on mount only
  useEffect(() => {
    const urlCategory = searchParams.get('category');
    const urlBrand    = searchParams.get('brand');
    if (urlCategory || urlBrand) {
      dispatch(setActiveFilters({
        category: urlCategory || null,
        brand:    urlBrand    || null,
      }));
    }
    dispatch(fetchCategoriesThunk());
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Re-fetch whenever filters OR page changes.
  // sortDescending is always included explicitly via buildApiParams.
  useEffect(() => {
    const params = buildApiParams(
      activeFilters,
      paged.pageNumber,
      paged.pageSize || 20,
    );
    dispatch(fetchProductsThunk(params));
  }, [dispatch, activeFilters, paged.pageNumber, paged.pageSize]);

  const handleFiltersApply = (filters) => {
    if (filters.category) {
      setSearchParams({ category: filters.category });
    } else {
      setSearchParams({});
    }
    setShowFilters(false);
  };

  const handlePageChange = (page) => {
    dispatch(setPage(page));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container fluid="xl" className="py-4 flex-grow-1">
        <div className="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
          <div>
            <h4 className="fw-bold mb-0">
              {activeFilters.category || 'All Products'}
            </h4>
            {!loading && (
              <small className="text-muted">
                {paged.totalCount} product{paged.totalCount !== 1 ? 's' : ''} found
              </small>
            )}
          </div>
          <Button
            variant="outline-secondary"
            size="sm"
            className="d-lg-none d-flex align-items-center gap-2"
            onClick={() => setShowFilters(true)}
          >
            <FiFilter /> Filters
          </Button>
        </div>

        <Row>
          <Col lg={3} className="d-none d-lg-block">
            <ProductFilters onApply={handleFiltersApply} />
          </Col>
          <Col lg={9}>
            <ProductGrid
              products={products}
              loading={loading}
              error={error}
              emptyMessage="No products match your filters."
            />
            <Pagination
              pageNumber={paged.pageNumber}
              totalPages={paged.totalPages}
              onPageChange={handlePageChange}
            />
          </Col>
        </Row>
      </Container>

      <Offcanvas
        show={showFilters}
        onHide={() => setShowFilters(false)}
        placement="start"
      >
        <Offcanvas.Header closeButton>
          <Offcanvas.Title>Filters</Offcanvas.Title>
        </Offcanvas.Header>
        <Offcanvas.Body>
          <ProductFilters onApply={handleFiltersApply} />
        </Offcanvas.Body>
      </Offcanvas>

      <Footer />
    </div>
  );
};

export default ProductListPage;