import { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Card, Form, Button, Accordion, Badge } from 'react-bootstrap';
import {
  setActiveFilters,
  resetFilters,
  selectActiveFilters,
  selectCategories,
} from '../../store/slices/productSlice.js';

const SORT_OPTIONS = [
  { label: 'Newest',              sortBy: 'createdAt', sortDescending: true  },
  { label: 'Price: Low to High',  sortBy: 'price',     sortDescending: false },
  { label: 'Price: High to Low',  sortBy: 'price',     sortDescending: true  },
  { label: 'Top Rated',           sortBy: 'rating',    sortDescending: true  },
  { label: 'Best Selling',        sortBy: 'soldCount', sortDescending: true  },
];

// Stable composite key so two options with same sortBy but different
// sortDescending are never treated as equal
const sortKey = (opt) => `${opt.sortBy}__${opt.sortDescending}`;

const emptyFilters = {
  category:      null,
  subCategory:   null,
  brand:         null,
  minPrice:      null,
  maxPrice:      null,
  minRating:     null,
  sortBy:        null,
  sortDescending: true,
};

const ProductFilters = ({ onApply }) => {
  const dispatch = useDispatch();
  const activeFilters  = useSelector(selectActiveFilters);
  const categories     = useSelector(selectCategories);

  // Keep local state in sync whenever Redux activeFilters changes
  // (e.g. external reset, category click from CategorySection)
  const [localFilters, setLocalFilters] = useState({ ...activeFilters });

  useEffect(() => {
    setLocalFilters({ ...activeFilters });
  }, [activeFilters]);

  const handleChange = (key, value) =>
    setLocalFilters((prev) => ({ ...prev, [key]: value }));

  const handleSortChange = (opt) =>
    setLocalFilters((prev) => ({
      ...prev,
      sortBy:         opt.sortBy,
      sortDescending: opt.sortDescending,
    }));

  const handleApply = () => {
    dispatch(setActiveFilters(localFilters));
    onApply?.(localFilters);
  };

  const handleReset = () => {
    dispatch(resetFilters());
    // local state will sync via the useEffect above, but set immediately
    // so the UI clears without waiting for the next render cycle
    setLocalFilters({ ...emptyFilters });
    onApply?.({});
  };

  // Count non-default active filters for the badge
  const activeCount = [
    localFilters.category,
    localFilters.brand,
    localFilters.minPrice,
    localFilters.maxPrice,
    localFilters.minRating,
    localFilters.sortBy,
  ].filter(Boolean).length;

  // Which sort option is currently selected?
  const activeSortKey = localFilters.sortBy
    ? `${localFilters.sortBy}__${localFilters.sortDescending}`
    : null;

  return (
    <Card className="shadow-sm border-0">
      <Card.Header className="bg-white d-flex justify-content-between align-items-center fw-bold">
        Filters
        {activeCount > 0 && (
          <Badge bg="primary" pill>{activeCount}</Badge>
        )}
      </Card.Header>

      <Card.Body className="p-2">
        <Accordion defaultActiveKey={['0', '1', '2', '3']} alwaysOpen flush>

          {/* ── Sort ──────────────────────────────────────────────── */}
          <Accordion.Item eventKey="0">
            <Accordion.Header>Sort By</Accordion.Header>
            <Accordion.Body className="pt-1 pb-2 px-1">
              {/* "Default" option to clear sort */}
              <Form.Check
                type="radio"
                id="sort-default"
                label="Default"
                name="sort"
                checked={!localFilters.sortBy}
                onChange={() =>
                  setLocalFilters((p) => ({
                    ...p,
                    sortBy: null,
                    sortDescending: true,
                  }))
                }
                className="mb-1"
              />
              {SORT_OPTIONS.map((opt) => (
                <Form.Check
                  key={sortKey(opt)}
                  type="radio"
                  id={`sort-${sortKey(opt)}`}
                  label={opt.label}
                  name="sort"
                  // Use composite key so Low/High and High/Low are distinct
                  checked={activeSortKey === sortKey(opt)}
                  onChange={() => handleSortChange(opt)}
                  className="mb-1"
                />
              ))}
            </Accordion.Body>
          </Accordion.Item>

          {/* ── Category ──────────────────────────────────────────── */}
          {categories.length > 0 && (
            <Accordion.Item eventKey="1">
              <Accordion.Header>Category</Accordion.Header>
              <Accordion.Body className="pt-1 pb-2 px-1">
                <Form.Check
                  type="radio"
                  id="cat-all"
                  label="All"
                  name="category"
                  checked={!localFilters.category}
                  onChange={() => handleChange('category', null)}
                  className="mb-1"
                />
                {categories.map((cat) => (
                  <Form.Check
                    key={cat.category}
                    type="radio"
                    id={`cat-${cat.category}`}
                    label={`${cat.category} (${cat.count})`}
                    name="category"
                    checked={localFilters.category === cat.category}
                    onChange={() => handleChange('category', cat.category)}
                    className="mb-1"
                  />
                ))}
              </Accordion.Body>
            </Accordion.Item>
          )}

          {/* ── Price Range ───────────────────────────────────────── */}
          <Accordion.Item eventKey="2">
            <Accordion.Header>Price Range</Accordion.Header>
            <Accordion.Body className="pt-1 pb-2 px-1">
              <div className="d-flex gap-2">
                <Form.Control
                  type="number"
                  placeholder="Min ₹"
                  size="sm"
                  value={localFilters.minPrice ?? ''}
                  onChange={(e) =>
                    handleChange('minPrice', e.target.value ? Number(e.target.value) : null)
                  }
                  min={0}
                />
                <Form.Control
                  type="number"
                  placeholder="Max ₹"
                  size="sm"
                  value={localFilters.maxPrice ?? ''}
                  onChange={(e) =>
                    handleChange('maxPrice', e.target.value ? Number(e.target.value) : null)
                  }
                  min={0}
                />
              </div>
            </Accordion.Body>
          </Accordion.Item>

          {/* ── Min Rating ────────────────────────────────────────── */}
          <Accordion.Item eventKey="3">
            <Accordion.Header>Min Rating</Accordion.Header>
            <Accordion.Body className="pt-1 pb-2 px-1">
              {[4, 3, 2, 1].map((r) => (
                <Form.Check
                  key={r}
                  type="radio"
                  id={`rating-${r}`}
                  label={`${'★'.repeat(r)}${'☆'.repeat(5 - r)} & up`}
                  name="minRating"
                  checked={localFilters.minRating === r}
                  onChange={() => handleChange('minRating', r)}
                  className="mb-1"
                />
              ))}
              <Form.Check
                type="radio"
                id="rating-any"
                label="Any rating"
                name="minRating"
                checked={!localFilters.minRating}
                onChange={() => handleChange('minRating', null)}
                className="mb-1"
              />
            </Accordion.Body>
          </Accordion.Item>

        </Accordion>

        <div className="d-grid gap-2 mt-3">
          <Button variant="primary" size="sm" onClick={handleApply}>
            Apply Filters
          </Button>
          <Button variant="outline-secondary" size="sm" onClick={handleReset}>
            Reset
          </Button>
        </div>
      </Card.Body>
    </Card>
  );
};

export default ProductFilters;