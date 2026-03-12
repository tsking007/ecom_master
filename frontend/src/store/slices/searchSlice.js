import { createSlice, createAsyncThunk, createSelector } from '@reduxjs/toolkit';
import { searchProductsApi } from '../../api/searchApi.js';

export const searchProductsThunk = createAsyncThunk(
  'search/searchProducts',
  async ({ term, page = 1, pageSize = 20 }, { rejectWithValue }) => {
    try {
      const res = await searchProductsApi(term, page, pageSize);
      return { ...res.data, term };
    } catch (err) {
      return rejectWithValue(err.response?.data?.detail || 'Search failed');
    }
  }
);

// ── Initial filter state ──────────────────────────────────────────────────────

const emptyFilters = {
  sortBy: null,         // 'price' | 'rating' | 'soldCount' | null
  sortDescending: true,
  minPrice: null,
  maxPrice: null,
  minRating: null,
};

// ── Slice ─────────────────────────────────────────────────────────────────────

const searchSlice = createSlice({
  name: 'search',
  initialState: {
    term: '',
    // Raw results from the API — never mutated, always the full page
    results: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 20,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
    loading: false,
    error: null,
    hasSearched: false,
    // Client-side filters applied on top of raw results
    filters: { ...emptyFilters },
  },
  reducers: {
    setSearchTerm: (state, action) => {
      state.term = action.payload;
    },
    clearSearch: (state) => {
      state.term = '';
      state.results = [];
      state.totalCount = 0;
      state.pageNumber = 1;
      state.totalPages = 0;
      state.hasNextPage = false;
      state.hasPreviousPage = false;
      state.error = null;
      state.hasSearched = false;
      state.filters = { ...emptyFilters };
    },
    setSearchPage: (state, action) => {
      state.pageNumber = action.payload;
    },
    setSearchFilters: (state, action) => {
      state.filters = { ...state.filters, ...action.payload };
    },
    resetSearchFilters: (state) => {
      state.filters = { ...emptyFilters };
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(searchProductsThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(searchProductsThunk.fulfilled, (state, action) => {
        state.loading = false;
        state.hasSearched = true;
        state.term         = action.payload.term;
        state.results      = action.payload.items;
        state.totalCount   = action.payload.totalCount;
        state.pageNumber   = action.payload.pageNumber;
        state.pageSize     = action.payload.pageSize;
        state.totalPages   = action.payload.totalPages;
        state.hasNextPage  = action.payload.hasNextPage;
        state.hasPreviousPage = action.payload.hasPreviousPage;
      })
      .addCase(searchProductsThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const {
  setSearchTerm,
  clearSearch,
  setSearchPage,
  setSearchFilters,
  resetSearchFilters,
} = searchSlice.actions;

export default searchSlice.reducer;

// ── Base selectors ────────────────────────────────────────────────────────────

export const selectSearchTerm        = (state) => state.search.term;
export const selectRawSearchResults  = (state) => state.search.results;
export const selectSearchLoading     = (state) => state.search.loading;
export const selectSearchError       = (state) => state.search.error;
export const selectSearchHasSearched = (state) => state.search.hasSearched;
export const selectSearchFilters     = (state) => state.search.filters;
export const selectSearchPaged       = (state) => ({
  totalCount:      state.search.totalCount,
  pageNumber:      state.search.pageNumber,
  pageSize:        state.search.pageSize,
  totalPages:      state.search.totalPages,
  hasNextPage:     state.search.hasNextPage,
  hasPreviousPage: state.search.hasPreviousPage,
});

// ── Derived selector: filter + sort applied client-side ───────────────────────
//
// Why client-side?
// The search backend (SearchController) only accepts term/page/pageSize.
// There are no server-side sort or filter params. Re-fetching on every
// filter change would discard the relevance ranking the search engine
// already did. Instead we filter/sort the current page's results locally.
// The user sees instant feedback with no extra network round-trips.
//
// Limitation: filters only apply within the current page. If the user
// is on page 2 and filters by price, they see filtered results from
// page 2 only — not a globally filtered dataset. This is an acceptable
// trade-off until the backend gains server-side filter params.

export const selectFilteredSearchResults = createSelector(
  [selectRawSearchResults, selectSearchFilters],
  (results, filters) => {
    let filtered = [...results];

    // ── Price range ───────────────────────────────────────────────────────
    if (filters.minPrice !== null && filters.minPrice !== undefined) {
      filtered = filtered.filter(
        (r) => r.effectivePrice >= filters.minPrice
      );
    }
    if (filters.maxPrice !== null && filters.maxPrice !== undefined) {
      filtered = filtered.filter(
        (r) => r.effectivePrice <= filters.maxPrice
      );
    }

    // ── Min rating ────────────────────────────────────────────────────────
    if (filters.minRating !== null && filters.minRating !== undefined) {
      filtered = filtered.filter(
        (r) => r.averageRating >= filters.minRating
      );
    }

    // ── Sort ──────────────────────────────────────────────────────────────
    if (filters.sortBy) {
      filtered.sort((a, b) => {
        let aVal, bVal;

        switch (filters.sortBy) {
          case 'price':
            aVal = a.effectivePrice;
            bVal = b.effectivePrice;
            break;
          case 'rating':
            aVal = a.averageRating;
            bVal = b.averageRating;
            break;
          case 'soldCount':
            aVal = a.soldCount;
            bVal = b.soldCount;
            break;
          default:
            return 0; // unknown sortBy — preserve original relevance order
        }

        // null/undefined values always sink to bottom regardless of direction
        if (aVal == null && bVal == null) return 0;
        if (aVal == null) return 1;
        if (bVal == null) return -1;

        return filters.sortDescending ? bVal - aVal : aVal - bVal;
      });
    }
    // No sortBy → preserve original relevance order from search engine

    return filtered;
  }
);

// Count of active non-default filters (for badge)
export const selectSearchFilterCount = createSelector(
  [selectSearchFilters],
  (filters) =>
    [filters.sortBy, filters.minPrice, filters.maxPrice, filters.minRating]
      .filter((v) => v !== null && v !== undefined).length
);