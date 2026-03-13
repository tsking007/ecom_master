import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { getOrderDetailsApi } from '../../api/ordersApi.js';

// ── Thunks ────────────────────────────────────────────────────────────────────

export const fetchOrderDetailsThunk = createAsyncThunk(
  'orders/fetchOrderDetails',
  async (orderId, { rejectWithValue }) => {
    try {
      const res = await getOrderDetailsApi(orderId);
      return res.data; // OrderDetailsDto
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.detail || 'Failed to fetch order details'
      );
    }
  }
);

// ── Slice ─────────────────────────────────────────────────────────────────────

const orderSlice = createSlice({
  name: 'orders',
  initialState: {
    // keyed by orderId for caching multiple orders
    byId: {},
    loading: false,
    error: null,
  },
  reducers: {
    clearOrderError: (state) => {
      state.error = null;
    },
    resetOrders: (state) => {
      state.byId = {};
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchOrderDetailsThunk.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchOrderDetailsThunk.fulfilled, (state, action) => {
        state.loading = false;
        // cache by orderId
        state.byId[action.payload.orderId] = action.payload;
      })
      .addCase(fetchOrderDetailsThunk.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export const { clearOrderError, resetOrders } = orderSlice.actions;
export default orderSlice.reducer;

// ── Selectors ─────────────────────────────────────────────────────────────────
export const selectOrderById = (orderId) => (state) =>
  state.orders.byId[orderId] || null;
export const selectOrdersLoading = (state) => state.orders.loading;
export const selectOrdersError = (state) => state.orders.error;