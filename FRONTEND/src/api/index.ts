import api from './client'

export const authApi = {
  register: (data: {
    full_name: string
    email: string
    phone: string
    password: string
  }) => api.post('/api/auth/register', data),

  login: (data: { email: string; password: string }) =>
    api.post('/api/auth/login', data),

  refresh: (data: { refresh_token: string }) =>
    api.post('/api/auth/refresh', data),

  logout: () => api.post('/api/auth/logout'),
}

export const productApi = {
  getProducts: (params?: {
    page?: number
    pageSize?: number
    category?: string
    brand?: string
    minPrice?: number
    maxPrice?: number
    search?: string
  }) => api.get('/api/products', { params }),

  getProductById: (id: number) => api.get(`/api/products/${id}`),

  getCategories: () => api.get('/api/products/categories'),

  getBrands: () => api.get('/api/products/brands'),
}

export const cartApi = {
  getCart: (userId: number) => api.get(`/api/cart?userId=${userId}`),

  addItem: (userId: number, data: { variant_id: number; quantity: number }) =>
    api.post(`/api/cart/items?userId=${userId}`, data),

  updateItem: (
    userId: number,
    itemId: number,
    data: { quantity: number }
  ) => api.put(`/api/cart/items/${itemId}?userId=${userId}`, data),

  removeItem: (userId: number, itemId: number) =>
    api.delete(`/api/cart/items/${itemId}?userId=${userId}`),

  applyVoucher: (userId: number, code: string) =>
    api.post(`/api/cart/apply-voucher?userId=${userId}&code=${code}`),

  clearCart: (userId: number) => api.post(`/api/cart/clear?userId=${userId}`),
}

export const orderApi = {
  createOrder: (
    userId: number,
    data: {
      paymentMethod: string
      shippingAddressJson: string
      note?: string
    }
  ) => api.post(`/api/orders?userId=${userId}`, data),

  getOrders: (userId: number, params?: { page?: number; pageSize?: number }) =>
    api.get(`/api/orders?userId=${userId}`, { params }),

  getOrderByCode: (userId: number, orderCode: string) =>
    api.get(`/api/orders/${orderCode}?userId=${userId}`),

  cancelOrder: (userId: number, orderId: number, reason: string) =>
    api.put(`/api/orders/${orderId}/cancel?userId=${userId}&reason=${reason}`),

  // Admin only
  getAdminOrders: (params?: { status?: string; page?: number; pageSize?: number }) =>
    api.get('/admin/orders', { params }),

  updateOrderStatus: (orderId: number, status: string) =>
    api.put(`/admin/orders/${orderId}/status`, { status }),
}
