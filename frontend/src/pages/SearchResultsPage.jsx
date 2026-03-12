import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSearchParams } from 'react-router-dom';
import {
  Container, Row, Col, Alert, Offcanvas,
  Button, Badge, Card, Form, Accordion,
} from 'react-bootstrap';
import { FiSearch, FiFilter, FiX } from 'react-icons/fi';
import AppNavbar from '../components/layout/Navbar.jsx';
import Footer from '../components/layout/Footer.jsx';
import ProductGrid from '../components/product/ProductGrid.jsx';
import Pagination from '../components/common/Pagination.jsx';
import {
  searchProductsThunk,
  setSearchPage,
  setSearchFilters,
  resetSearchFilters,
  selectSearchTerm,
  selectSearchLoading,
  selectSearchError,
  selectSearchHasSearched,
  selectSearchPaged,
  selectSearchFilters,
  selectFilteredSearchResults,
  selectSearchFilterCount,
} from '../store/slices/searchSlice.js';

// ── Sort options ──────────────────────────────────────────────────────────────
// No 'createdAt' here — search results have a relevance score from the
// search engine that acts as the natural default order. Overriding it
// with createdAt would destroy relevance ranking.
const SORT_OPTIONS = [
  { label: 'Relevance (default)', sortBy: null,        sortDescending: true  },
  { label: 'Price: Low to High',  sortBy: 'price',     sortDescending: false },
  { label: 'Price: High to Low',  sortBy: 'price',     sortDescending: true  },
  { label: 'Top Rated',           sortBy: 'rating',    sortDescending: true  },
  { label: 'Best Selling',        sortBy: 'soldCount', sortDescending: true  },
];

const sortKey = (opt) => `${opt.sortBy}__${opt.sortDescending}`;

// ── Inline filter panel ───────────────────────────────────────────────────────
// Kept local to this page — search filters are different from product
// catalogue filters (no category, has relevance sort option).

const emptyLocal = {
  sortBy: null,
  sortDescending: true,
  minPrice: null,
  maxPrice: null,
  minRating: null,
};

const SearchFilterPanel = ({ onApply, onClose }) => {
  const dispatch      = useDispatch();
  const activeFilters = useSelector(selectSearchFilters);
  const filterCount   = useSelector(selectSearchFilterCount);

  const [local, setLocal] = useState({ ...activeFilters });

  // Stay in sync if filters are reset externally
  useEffect(() => {
    setLocal({ ...activeFilters });
  }, [activeFilters]);

  const activeSortKey = local.sortBy
    ? `${local.sortBy}__${local.sortDescending}`
    : 'null__true'; // matches the "Relevance" option

  const handleApply = () => {
    dispatch(setSearchFilters(local));
    onApply?.();
  };

  const handleReset = () => {
    dispatch(resetSearchFilters());
    setLocal({ ...emptyLocal });
    onApply?.();
  };

  return (
    <Card className="border-0 shadow-sm">
      <Card.Header className="bg-white d-flex justify-content-between align-items-center fw-bold">
        <span>
          Filter Results
          {filterCount > 0 && (
            <Badge bg="primary" pill className="ms-2">{filterCount}</Badge>
          )}
        </span>
        {/* Only shown inside Offcanvas on mobile */}
        {onClose && (
          <Button variant="link" className="p-0 text-muted" onClick={onClose}>
            <FiX size={18} />
          </Button>
        )}
      </Card.Header>

      <Card.Body className="p-2">
        <Accordion defaultActiveKey={['0', '1', '2']} alwaysOpen flush>

          {/* ── Sort ──────────────────────────────────────────────── */}
          <Accordion.Item eventKey="0">
            <Accordion.Header>Sort By</Accordion.Header>
            <Accordion.Body className="pt-1 pb-2 px-1">
              {SORT_OPTIONS.map((opt) => {
                const key = sortKey(opt);
                // "Relevance" option: checked when no sortBy is set
                const isChecked = opt.sortBy === null
                  ? local.sortBy === null
                  : activeSortKey === key;

                return (
                  <Form.Check
                    key={key}
                    type="radio"
                    id={`search-sort-${key}`}
                    label={opt.label}
                    name="searchSort"
                    checked={isChecked}
                    onChange={() =>
                      setLocal((p) => ({
                        ...p,
                        sortBy: opt.sortBy,
                        sortDescending: opt.sortDescending,
                      }))
                    }
                    className="mb-1"
                  />
                );
              })}
            </Accordion.Body>
          </Accordion.Item>

          {/* ── Price Range ───────────────────────────────────────── */}
          <Accordion.Item eventKey="1">
            <Accordion.Header>Price Range</Accordion.Header>
            <Accordion.Body className="pt-1 pb-2 px-1">
              <div className="d-flex gap-2">
                <Form.Control
                  type="number"
                  placeholder="Min ₹"
                  size="sm"
                  value={local.minPrice ?? ''}
                  min={0}
                  onChange={(e) =>
                    setLocal((p) => ({
                      ...p,
                      minPrice: e.target.value ? Number(e.target.value) : null,
                    }))
                  }
                />
                <Form.Control
                  type="number"
                  placeholder="Max ₹"
                  size="sm"
                  value={local.maxPrice ?? ''}
                  min={0}
                  onChange={(e) =>
                    setLocal((p) => ({
                      ...p,
                      maxPrice: e.target.value ? Number(e.target.value) : null,
                    }))
                  }
                />
              </div>
              {/* Show a hint when both are set and min > max */}
              {local.minPrice !== null &&
               local.maxPrice !== null &&
               local.minPrice > local.maxPrice && (
                <small className="text-danger mt-1 d-block">
                  Min price cannot exceed max price
                </small>
              )}
            </Accordion.Body>
          </Accordion.Item>

          {/* ── Min Rating ────────────────────────────────────────── */}
          <Accordion.Item eventKey="2">
            <Accordion.Header>Min Rating</Accordion.Header>
            <Accordion.Body className="pt-1 pb-2 px-1">
              {[4, 3, 2, 1].map((r) => (
                <Form.Check
                  key={r}
                  type="radio"
                  id={`search-rating-${r}`}
                  label={`${'★'.repeat(r)}${'☆'.repeat(5 - r)} & up`}
                  name="searchRating"
                  checked={local.minRating === r}
                  onChange={() => setLocal((p) => ({ ...p, minRating: r }))}
                  className="mb-1"
                />
              ))}
              <Form.Check
                type="radio"
                id="search-rating-any"
                label="Any rating"
                name="searchRating"
                checked={local.minRating === null}
                onChange={() => setLocal((p) => ({ ...p, minRating: null }))}
                className="mb-1"
              />
            </Accordion.Body>
          </Accordion.Item>

        </Accordion>

        <div className="d-grid gap-2 mt-3">
          <Button
            variant="primary"
            size="sm"
            onClick={handleApply}
            disabled={
              // Prevent applying when min > max
              local.minPrice !== null &&
              local.maxPrice !== null &&
              local.minPrice > local.maxPrice
            }
          >
            Apply
          </Button>
          <Button variant="outline-secondary" size="sm" onClick={handleReset}>
            Reset
          </Button>
        </div>
      </Card.Body>
    </Card>
  );
};

// ── Active filter chips ───────────────────────────────────────────────────────
// Shown below the heading so the user can see and remove active filters
// without opening the panel.

const FilterChips = () => {
  const dispatch = useDispatch();
  const filters  = useSelector(selectSearchFilters);

  const chips = [];

  if (filters.sortBy) {
    const opt = SORT_OPTIONS.find(
      (o) => o.sortBy === filters.sortBy && o.sortDescending === filters.sortDescending
    );
    chips.push({
      key: 'sort',
      label: opt?.label ?? `Sort: ${filters.sortBy}`,
      onRemove: () => dispatch(setSearchFilters({ sortBy: null, sortDescending: true })),
    });
  }

  if (filters.minPrice !== null) {
    chips.push({
      key: 'minPrice',
      label: `Min ₹${filters.minPrice.toLocaleString('en-IN')}`,
      onRemove: () => dispatch(setSearchFilters({ minPrice: null })),
    });
  }

  if (filters.maxPrice !== null) {
    chips.push({
      key: 'maxPrice',
      label: `Max ₹${filters.maxPrice.toLocaleString('en-IN')}`,
      onRemove: () => dispatch(setSearchFilters({ maxPrice: null })),
    });
  }

  if (filters.minRating !== null) {
    chips.push({
      key: 'minRating',
      label: `${'★'.repeat(filters.minRating)} & up`,
      onRemove: () => dispatch(setSearchFilters({ minRating: null })),
    });
  }

  if (!chips.length) return null;

  return (
    <div className="d-flex flex-wrap gap-2 mb-3">
      {chips.map((chip) => (
        <Badge
          key={chip.key}
          bg="light"
          text="dark"
          className="d-flex align-items-center gap-1 border px-2 py-1"
          style={{ fontSize: '0.8rem', fontWeight: 500 }}
        >
          {chip.label}
          <FiX
            size={12}
            style={{ cursor: 'pointer', marginLeft: '2px' }}
            onClick={chip.onRemove}
          />
        </Badge>
      ))}
      <Badge
        bg="danger"
        className="d-flex align-items-center gap-1 px-2 py-1"
        style={{ fontSize: '0.8rem', cursor: 'pointer' }}
        onClick={() => dispatch(resetSearchFilters())}
      >
        Clear all
      </Badge>
    </div>
  );
};

// ── Main page ─────────────────────────────────────────────────────────────────

const SearchResultsPage = () => {
  const dispatch      = useDispatch();
  const [searchParams] = useSearchParams();
  const urlTerm       = searchParams.get('q') || '';

  const [showFilters, setShowFilters] = useState(false);

  const results      = useSelector(selectFilteredSearchResults); // client-filtered
  const loading      = useSelector(selectSearchLoading);
  const error        = useSelector(selectSearchError);
  const hasSearched  = useSelector(selectSearchHasSearched);
  const paged        = useSelector(selectSearchPaged);
  const currentTerm  = useSelector(selectSearchTerm);
  const filterCount  = useSelector(selectSearchFilterCount);

  // Fire search when URL param changes
  useEffect(() => {
    if (urlTerm) {
      dispatch(searchProductsThunk({ term: urlTerm, page: 1 }));
    }
  }, [dispatch, urlTerm]);

  const handlePageChange = (page) => {
    dispatch(setSearchPage(page));
    dispatch(searchProductsThunk({ term: currentTerm, page }));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Result count label — shows filtered count vs total when filters active
  const rawTotal      = paged.totalCount;
  const filteredCount = results.length;
  const isFiltered    = filterCount > 0;

  const resultLabel = loading
    ? null
    : isFiltered
      ? `${filteredCount} of ${rawTotal} result${rawTotal !== 1 ? 's' : ''} (filtered)`
      : `${rawTotal} result${rawTotal !== 1 ? 's' : ''} found`;

  return (
    <div className="d-flex flex-column min-vh-100">
      <AppNavbar />

      <Container fluid="xl" className="py-4 flex-grow-1">

        {/* ── Page header ──────────────────────────────────────── */}
        <div className="d-flex justify-content-between align-items-start mb-2 flex-wrap gap-2">
          <div>
            <h4 className="fw-bold mb-0 d-flex align-items-center gap-2">
              <FiSearch className="text-muted" />
              {urlTerm ? `Results for "${urlTerm}"` : 'Search'}
            </h4>
            {resultLabel && (
              <small className="text-muted">{resultLabel}</small>
            )}
          </div>

          {/* Mobile filter toggle */}
          <Button
            variant="outline-secondary"
            size="sm"
            className="d-lg-none d-flex align-items-center gap-2"
            onClick={() => setShowFilters(true)}
          >
            <FiFilter />
            Filters
            {filterCount > 0 && (
              <Badge bg="primary" pill style={{ fontSize: '0.65rem' }}>
                {filterCount}
              </Badge>
            )}
          </Button>
        </div>

        {/* ── Active filter chips ───────────────────────────────── */}
        <FilterChips />

        {error && <Alert variant="danger" className="mb-3">{error}</Alert>}

        <Row>
          {/* ── Sidebar — desktop only ────────────────────────── */}
          <Col lg={3} className="d-none d-lg-block">
            <SearchFilterPanel onApply={() => {}} />
          </Col>

          {/* ── Results grid ──────────────────────────────────── */}
          <Col lg={9}>
            {/* Show a note when filters reduced results to 0 but raw had some */}
            {!loading && hasSearched && filteredCount === 0 && rawTotal > 0 && (
              <Alert variant="warning" className="py-2">
                No results match your current filters.{' '}
                <span
                  className="fw-semibold text-decoration-underline"
                  style={{ cursor: 'pointer' }}
                  onClick={() => dispatch(resetSearchFilters())}
                >
                  Clear filters
                </span>{' '}
                to see all {rawTotal} result{rawTotal !== 1 ? 's' : ''}.
              </Alert>
            )}

            <ProductGrid
              products={results}
              loading={loading}
              error={null}
              emptyMessage={
                hasSearched
                  ? `No products found for "${urlTerm}".`
                  : 'Enter a search term to find products.'
              }
            />

            {/* Only show pagination when no client filters are active.
                With client-side filtering, pagination operates on raw
                pages from the server. Showing page controls while
                filters are active would be confusing because page 2
                might have results that pass the filter but page 1's
                filtered view appears empty. */}
            {!isFiltered && (
              <Pagination
                pageNumber={paged.pageNumber}
                totalPages={paged.totalPages}
                onPageChange={handlePageChange}
              />
            )}

            {/* When filters are active, show a note about pagination */}
            {isFiltered && paged.totalPages > 1 && (
              <p className="text-center text-muted small mt-3">
                Filters apply to the current page only.{' '}
                <span
                  className="text-primary"
                  style={{ cursor: 'pointer' }}
                  onClick={() => dispatch(resetSearchFilters())}
                >
                  Clear filters
                </span>{' '}
                to paginate through all results.
              </p>
            )}
          </Col>
        </Row>
      </Container>

      {/* ── Mobile filter offcanvas ───────────────────────────────── */}
      <Offcanvas
        show={showFilters}
        onHide={() => setShowFilters(false)}
        placement="start"
      >
        <Offcanvas.Header closeButton>
          <Offcanvas.Title>
            Filter Results
            {filterCount > 0 && (
              <Badge bg="primary" pill className="ms-2">{filterCount}</Badge>
            )}
          </Offcanvas.Title>
        </Offcanvas.Header>
        <Offcanvas.Body>
          <SearchFilterPanel
            onApply={() => setShowFilters(false)}
            onClose={() => setShowFilters(false)}
          />
        </Offcanvas.Body>
      </Offcanvas>

      <Footer />
    </div>
  );
};

export default SearchResultsPage;