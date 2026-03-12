import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import {
  getProductsApi,
  getCategoriesApi,
  getBestsellersApi,
  getProductBySlugApi,
} from '../../api/productApi.js';

// ── Thunks ────────────────────────────────────────────────────────────────────

export const fetchProductsThunk = createAsyncThunk(
  'products/fetchProducts',
  async (params, { rejectWithValue }) => {
    try {
      const res = await getProductsApi(params);
      return res.data; // PagedResult<ProductListDto>
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || err.response?.data?.title || 'Failed to fetch products'
      );
    }
  }
);

export const fetchCategoriesThunk = createAsyncThunk(
  'products/fetchCategories',
  async (_, { rejectWithValue }) => {
    try {
      const res = await getCategoriesApi();
      return res.data; // CategoryDto[]
    } catch (err) {
      return rejectWithValue('Failed to fetch categories');
    }
  }
);

export const fetchBestsellersThunk = createAsyncThunk(
  'products/fetchBestsellers',
  async (count = 8, { rejectWithValue }) => {
    try {
      const res = await getBestsellersApi(count);
      return res.data; // ProductListDto[]
    } catch (err) {
      return rejectWithValue('Failed to fetch bestsellers');
    }
  }
);

export const fetchProductBySlugThunk = createAsyncThunk(
  'products/fetchBySlug',
  async (slug, { rejectWithValue }) => {
    try {
      const res = await getProductBySlugApi(slug);
      return res.data; // ProductDto
    } catch (err) {
      return rejectWithValue(
        err.response?.status === 404
          ? 'Product not found'
          : 'Failed to fetch product'
      );
    }
  }
);

// ── Slice ─────────────────────────────────────────────────────────────────────

const productSlice = createSlice({
  name: 'products',
  initialState: {
    // Product list / filters page
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 20,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
    listLoading: false,
    listError: null,

    // Categories
    categories: [],
    categoriesLoading: false,

    // Bestsellers
    bestsellers: [],
    bestsellersLoading: false,

    // Single product detail
    selectedProduct: null,
    detailLoading: false,
    detailError: null,

    // Active filters (kept in slice so Navbar/Filters stay in sync)
    activeFilters: {
      category: null,
      subCategory: null,
      brand: null,
      minPrice: null,
      maxPrice: null,
      minRating: null,
      sortBy: null,
      sortDescending: true,
    },
  },
  reducers: {
    setActiveFilters: (state, action) => {
  const incoming = action.payload;

  // If the caller is setting sortBy, they MUST also explicitly set
  // sortDescending in the same payload — never inherit the old value
  // when switching sort fields, because the old direction is meaningless
  // for the new field. ProductFilters always sends both together, but
  // this guards against partial callers (e.g. CategorySection).
  const next = { ...state.activeFilters, ...incoming };

  // If sortBy was explicitly cleared, also reset sortDescending to default
  if ('sortBy' in incoming && incoming.sortBy === null) {
    next.sortDescending = true;
  }

  state.activeFilters = next;
  state.pageNumber = 1;
},
    resetFilters: (state) => {
      state.activeFilters = {
        category: null,
        subCategory: null,
        brand: null,
        minPrice: null,
        maxPrice: null,
        minRating: null,
        sortBy: null,
        sortDescending: true,
      };
      state.pageNumber = 1;
    },
    setPage: (state, action) => {
      state.pageNumber = action.payload;
    },
    clearSelectedProduct: (state) => {
      state.selectedProduct = null;
      state.detailError = null;
    },
  },
  extraReducers: (builder) => {
    // ── Fetch products list ──────────────────────────────────────────────────
    builder
      .addCase(fetchProductsThunk.pending, (state) => {
        state.listLoading = true;
        state.listError = null;
      })
      .addCase(fetchProductsThunk.fulfilled, (state, action) => {
        state.listLoading = false;
        const { items, totalCount, pageNumber, pageSize, totalPages, hasNextPage, hasPreviousPage } =
          action.payload;
        state.items = items;
        state.totalCount = totalCount;
        state.pageNumber = pageNumber;
        state.pageSize = pageSize;
        state.totalPages = totalPages;
        state.hasNextPage = hasNextPage;
        state.hasPreviousPage = hasPreviousPage;
      })
      .addCase(fetchProductsThunk.rejected, (state, action) => {
        state.listLoading = false;
        state.listError = action.payload;
      });

    // ── Fetch categories ─────────────────────────────────────────────────────
    builder
      .addCase(fetchCategoriesThunk.pending, (state) => {
        state.categoriesLoading = true;
      })
      .addCase(fetchCategoriesThunk.fulfilled, (state, action) => {
        state.categoriesLoading = false;
        state.categories = action.payload;
      })
      .addCase(fetchCategoriesThunk.rejected, (state) => {
        state.categoriesLoading = false;
      });

    // ── Fetch bestsellers ────────────────────────────────────────────────────
    builder
      .addCase(fetchBestsellersThunk.pending, (state) => {
        state.bestsellersLoading = true;
      })
      .addCase(fetchBestsellersThunk.fulfilled, (state, action) => {
        state.bestsellersLoading = false;
        state.bestsellers = action.payload;
      })
      .addCase(fetchBestsellersThunk.rejected, (state) => {
        state.bestsellersLoading = false;
      });

    // ── Fetch single product ─────────────────────────────────────────────────
    builder
      .addCase(fetchProductBySlugThunk.pending, (state) => {
        state.detailLoading = true;
        state.detailError = null;
        state.selectedProduct = null;
      })
      .addCase(fetchProductBySlugThunk.fulfilled, (state, action) => {
        state.detailLoading = false;
        state.selectedProduct = action.payload;
      })
      .addCase(fetchProductBySlugThunk.rejected, (state, action) => {
        state.detailLoading = false;
        state.detailError = action.payload;
      });
  },
});

export const { setActiveFilters, resetFilters, setPage, clearSelectedProduct } =
  productSlice.actions;
export default productSlice.reducer;

// ── Selectors ─────────────────────────────────────────────────────────────────
export const selectProducts = (state) => state.products.items;
export const selectProductsPaged = (state) => ({
  totalCount: state.products.totalCount,
  pageNumber: state.products.pageNumber,
  pageSize: state.products.pageSize,
  totalPages: state.products.totalPages,
  hasNextPage: state.products.hasNextPage,
  hasPreviousPage: state.products.hasPreviousPage,
});
export const selectListLoading = (state) => state.products.listLoading;
export const selectListError = (state) => state.products.listError;
export const selectCategories = (state) => state.products.categories;
export const selectBestsellers = (state) => state.products.bestsellers;
export const selectBestsellersLoading = (state) => state.products.bestsellersLoading;
export const selectSelectedProduct = (state) => state.products.selectedProduct;
export const selectDetailLoading = (state) => state.products.detailLoading;
export const selectDetailError = (state) => state.products.detailError;
export const selectActiveFilters = (state) => state.products.activeFilters;