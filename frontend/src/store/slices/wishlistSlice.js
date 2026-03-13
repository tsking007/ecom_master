import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import {
  getWishlistApi,
  addToWishlistApi,
  removeWishlistItemApi,
  moveToCartApi,
} from '../../api/wishlistApi.js';
import { fetchCartThunk } from './cartSlice.js';

// ── Thunks ────────────────────────────────────────────────────────────────────

export const fetchWishlistThunk = createAsyncThunk(
  'wishlist/fetchWishlist',
  async (_, { rejectWithValue }) => {
    try {
      const res = await getWishlistApi();
      return res.data; // WishlistDto
    } catch (err) {
      return rejectWithValue('Failed to fetch wishlist');
    }
  }
);

export const addToWishlistThunk = createAsyncThunk(
  'wishlist/addToWishlist',
  async (productId, { dispatch, rejectWithValue }) => {
    try {
      await addToWishlistApi(productId);
      dispatch(fetchWishlistThunk());
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || 'Failed to add to wishlist'
      );
    }
  }
);

export const removeWishlistItemThunk = createAsyncThunk(
  'wishlist/removeItem',
  async (wishlistId, { dispatch, rejectWithValue }) => {
    try {
      await removeWishlistItemApi(wishlistId);
      dispatch(fetchWishlistThunk());
    } catch (err) {
      return rejectWithValue('Failed to remove wishlist item');
    }
  }
);

export const moveToCartThunk = createAsyncThunk(
  'wishlist/moveToCart',
  async (wishlistId, { dispatch, rejectWithValue }) => {
    try {
      await moveToCartApi(wishlistId);
      // Sync both wishlist and cart after move
      dispatch(fetchWishlistThunk());
      dispatch(fetchCartThunk());
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || 'Failed to move item to cart'
      );
    }
  }
);

// ── Slice ─────────────────────────────────────────────────────────────────────

const wishlistSlice = createSlice({
  name: 'wishlist',
  initialState: {
    items: [],
    totalItems: 0,
    hasOutOfStockItems: false,
    loading: false,
    mutating: false,
    error: null,
  },
  reducers: {
    clearWishlistError: (state) => {
      state.error = null;
    },
    resetWishlist: (state) => {
      state.items = [];
      state.totalItems = 0;
      state.hasOutOfStockItems = false;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // ── Fetch ────────────────────────────────────────────────────────────────
    builder
      .addCase(fetchWishlistThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchWishlistThunk.fulfilled, (state, action) => {
        state.loading = false;
        const { items, totalItems, hasOutOfStockItems } = action.payload;
        state.items = items;
        state.totalItems = totalItems;
        state.hasOutOfStockItems = hasOutOfStockItems;
      })
      .addCase(fetchWishlistThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });

    // ── Add ──────────────────────────────────────────────────────────────────
    builder
      .addCase(addToWishlistThunk.pending, (state) => {
        state.mutating = true;
        state.error = null;
      })
      .addCase(addToWishlistThunk.fulfilled, (state) => {
        state.mutating = false;
      })
      .addCase(addToWishlistThunk.rejected, (state, action) => {
        state.mutating = false;
        state.error = action.payload;
      });

    // ── Remove ───────────────────────────────────────────────────────────────
    builder
      .addCase(removeWishlistItemThunk.pending, (state) => { state.mutating = true; })
      .addCase(removeWishlistItemThunk.fulfilled, (state) => { state.mutating = false; })
      .addCase(removeWishlistItemThunk.rejected, (state, action) => {
        state.mutating = false;
        state.error = action.payload;
      });

    // ── Move to cart ─────────────────────────────────────────────────────────
    builder
      .addCase(moveToCartThunk.pending, (state) => { state.mutating = true; })
      .addCase(moveToCartThunk.fulfilled, (state) => { state.mutating = false; })
      .addCase(moveToCartThunk.rejected, (state, action) => {
        state.mutating = false;
        state.error = action.payload;
      });
  },
});

export const { clearWishlistError, resetWishlist } = wishlistSlice.actions;
export default wishlistSlice.reducer;

// ── Selectors ─────────────────────────────────────────────────────────────────
export const selectWishlistItems = (state) => state.wishlist.items;
export const selectWishlistTotalItems = (state) => state.wishlist.totalItems;
export const selectWishlistLoading = (state) => state.wishlist.loading;
export const selectWishlistMutating = (state) => state.wishlist.mutating;
export const selectWishlistError = (state) => state.wishlist.error;
export const selectHasOutOfStockWishlistItems = (state) => state.wishlist.hasOutOfStockItems;