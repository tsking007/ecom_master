import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import {
  getCartApi,
  addToCartApi,
  updateCartItemApi,
  removeCartItemApi,
  clearCartApi,
} from '../../api/cartApi.js';

// ── Thunks ────────────────────────────────────────────────────────────────────

export const fetchCartThunk = createAsyncThunk(
  'cart/fetchCart',
  async (_, { rejectWithValue }) => {
    try {
      const res = await getCartApi();
      return res.data; // CartDto
    } catch (err) {
      return rejectWithValue('Failed to fetch cart');
    }
  }
);

export const addToCartThunk = createAsyncThunk(
  'cart/addToCart',
  async ({ productId, quantity }, { dispatch, rejectWithValue }) => {
    try {
      await addToCartApi(productId, quantity);
      dispatch(fetchCartThunk()); // re-sync cart after mutation
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail ||
        err.response?.data?.errors?.Quantity?.[0] ||
        err.response?.data?.errors?.ProductId?.[0] ||
        'Failed to add to cart'
      );
    }
  }
);

export const updateCartItemThunk = createAsyncThunk(
  'cart/updateItem',
  async ({ cartItemId, quantity }, { dispatch, rejectWithValue }) => {
    try {
      await updateCartItemApi(cartItemId, quantity);
      dispatch(fetchCartThunk());
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || 'Failed to update cart item'
      );
    }
  }
);

export const removeCartItemThunk = createAsyncThunk(
  'cart/removeItem',
  async (cartItemId, { dispatch, rejectWithValue }) => {
    try {
      await removeCartItemApi(cartItemId);
      dispatch(fetchCartThunk());
    } catch (err) {
      return rejectWithValue('Failed to remove cart item');
    }
  }
);

export const clearCartThunk = createAsyncThunk(
  'cart/clearCart',
  async (_, { dispatch, rejectWithValue }) => {
    try {
      await clearCartApi();
      dispatch(fetchCartThunk());
    } catch (err) {
      return rejectWithValue('Failed to clear cart');
    }
  }
);

// ── Slice ─────────────────────────────────────────────────────────────────────

const cartSlice = createSlice({
  name: 'cart',
  initialState: {
    cartId: null,
    items: [],
    totalItems: 0,
    subtotal: 0,
    hasOutOfStockItems: false,
    hasPriceChanges: false,
    loading: false,
    mutating: false, // add/update/remove in progress
    error: null,
  },
  reducers: {
    clearCartError: (state) => {
      state.error = null;
    },
    resetCart: (state) => {
      state.cartId = null;
      state.items = [];
      state.totalItems = 0;
      state.subtotal = 0;
      state.hasOutOfStockItems = false;
      state.hasPriceChanges = false;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // ── Fetch cart ───────────────────────────────────────────────────────────
    builder
      .addCase(fetchCartThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchCartThunk.fulfilled, (state, action) => {
        state.loading = false;
        const { cartId, items, totalItems, subtotal, hasOutOfStockItems, hasPriceChanges } =
          action.payload;
        state.cartId = cartId;
        state.items = items;
        state.totalItems = totalItems;
        state.subtotal = subtotal;
        state.hasOutOfStockItems = hasOutOfStockItems;
        state.hasPriceChanges = hasPriceChanges;
      })
      .addCase(fetchCartThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });

    // ── Add to cart ──────────────────────────────────────────────────────────
    builder
      .addCase(addToCartThunk.pending, (state) => {
        state.mutating = true;
        state.error = null;
      })
      .addCase(addToCartThunk.fulfilled, (state) => {
        state.mutating = false;
      })
      .addCase(addToCartThunk.rejected, (state, action) => {
        state.mutating = false;
        state.error = action.payload;
      });

    // ── Update item ──────────────────────────────────────────────────────────
    builder
      .addCase(updateCartItemThunk.pending, (state) => { state.mutating = true; })
      .addCase(updateCartItemThunk.fulfilled, (state) => { state.mutating = false; })
      .addCase(updateCartItemThunk.rejected, (state, action) => {
        state.mutating = false;
        state.error = action.payload;
      });

    // ── Remove item ──────────────────────────────────────────────────────────
    builder
      .addCase(removeCartItemThunk.pending, (state) => { state.mutating = true; })
      .addCase(removeCartItemThunk.fulfilled, (state) => { state.mutating = false; })
      .addCase(removeCartItemThunk.rejected, (state, action) => {
        state.mutating = false;
        state.error = action.payload;
      });

    // ── Clear cart ───────────────────────────────────────────────────────────
    builder
      .addCase(clearCartThunk.pending, (state) => { state.mutating = true; })
      .addCase(clearCartThunk.fulfilled, (state) => { state.mutating = false; })
      .addCase(clearCartThunk.rejected, (state, action) => {
        state.mutating = false;
        state.error = action.payload;
      });
  },
});

export const { clearCartError, resetCart } = cartSlice.actions;
export default cartSlice.reducer;

// ── Selectors ─────────────────────────────────────────────────────────────────
export const selectCartItems = (state) => state.cart.items;
export const selectCartTotalItems = (state) => state.cart.totalItems;
export const selectCartSubtotal = (state) => state.cart.subtotal;
export const selectCartLoading = (state) => state.cart.loading;
export const selectCartMutating = (state) => state.cart.mutating;
export const selectCartError = (state) => state.cart.error;
export const selectHasOutOfStockItems = (state) => state.cart.hasOutOfStockItems;
export const selectHasPriceChanges = (state) => state.cart.hasPriceChanges;