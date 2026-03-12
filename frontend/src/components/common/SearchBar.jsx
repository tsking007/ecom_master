import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Form, InputGroup, Button } from 'react-bootstrap';
import { FiSearch, FiX } from 'react-icons/fi';
import {
  searchProductsThunk,
  setSearchTerm,
  clearSearch,
  selectSearchTerm,
  selectSearchLoading,
} from '../../store/slices/searchSlice.js';

const SearchBar = ({ autoFocus = false, size = 'md' }) => {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const storedTerm = useSelector(selectSearchTerm);
  const loading = useSelector(selectSearchLoading);

  const [localTerm, setLocalTerm] = useState(storedTerm || '');

  const handleSearch = (e) => {
    e.preventDefault();
    const trimmed = localTerm.trim();
    if (!trimmed) return;
    dispatch(setSearchTerm(trimmed));
    dispatch(searchProductsThunk({ term: trimmed, page: 1 }));
    navigate(`/search?q=${encodeURIComponent(trimmed)}`);
  };

  const handleClear = () => {
    setLocalTerm('');
    dispatch(clearSearch());
  };

  return (
    <Form onSubmit={handleSearch} className="w-100">
      <InputGroup size={size}>
        <Form.Control
          type="text"
          placeholder="Search products..."
          value={localTerm}
          onChange={(e) => setLocalTerm(e.target.value)}
          autoFocus={autoFocus}
          aria-label="Search products"
        />
        {localTerm && (
          <Button
            variant="outline-secondary"
            type="button"
            onClick={handleClear}
            aria-label="Clear search"
          >
            <FiX />
          </Button>
        )}
        <Button
          variant="primary"
          type="submit"
          disabled={loading || !localTerm.trim()}
          aria-label="Submit search"
        >
          {loading ? (
            <span className="spinner-border spinner-border-sm" role="status" />
          ) : (
            <FiSearch />
          )}
        </Button>
      </InputGroup>
    </Form>
  );
};

export default SearchBar;