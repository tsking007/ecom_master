import axiosInstance from './axiosInstance.js';

const ADDRESS_BASE = '/api/v1/addresses';

// GET /api/v1/addresses
// Response: AddressDto[]
export const getAddressesApi = () =>
  axiosInstance.get(ADDRESS_BASE);

// POST /api/v1/addresses
export const addAddressApi = (data) =>
  axiosInstance.post(ADDRESS_BASE, data);

// PUT /api/v1/addresses/{addressId}
export const updateAddressApi = (addressId, data) =>
  axiosInstance.put(`${ADDRESS_BASE}/${addressId}`, data);

// DELETE /api/v1/addresses/{addressId}
export const deleteAddressApi = (addressId) =>
  axiosInstance.delete(`${ADDRESS_BASE}/${addressId}`);

// PATCH /api/v1/addresses/{addressId}/set-default
export const setDefaultAddressApi = (addressId) =>
  axiosInstance.patch(`${ADDRESS_BASE}/${addressId}/set-default`);