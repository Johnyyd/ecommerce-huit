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
    api.post('/api/auth/refresh-token', data),

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
  getCart: (userId: number) => api.get('/api/cart'),

  addItem: (userId: number, data: { variantId: number; quantity: number }) =>
    api.post('/api/cart/items', data),

  updateItem: (userId: number, itemId: number, data: { quantity: number }) =>
    api.put(`/api/cart/items/${itemId}`, data),

  removeItem: (userId: number, itemId: number) =>
    api.delete(`/api/cart/items/${itemId}`),

  applyVoucher: (userId: number, code: string) =>
    api.post('/api/cart/apply-voucher', { code }),

  clearCart: (userId: number) => api.delete('/api/cart/clear'),
}

export const orderApi = {
  createOrder: (
    userId: number,
    data: {
      paymentMethod: string
      shippingAddressJson: string
      note?: string
    }
  ) => api.post('/api/orders', data),

  getOrders: (userId: number, params?: { page?: number; pageSize?: number }) =>
    api.get('/api/orders', { params }),

  getOrderByCode: (userId: number, orderCode: string) =>
    api.get(`/api/orders/${orderCode}`),

  cancelOrder: (userId: number, orderId: number, reason: string) =>
    api.post(`/api/orders/${orderId}/cancel`, { reason }),

  // Admin only
  getAdminOrders: (params?: { status?: string; page?: number; pageSize?: number }) =>
    api.get('/admin/orders', { params }),

  updateOrderStatus: (orderId: number, status: string) =>
    api.put(`/admin/orders/${orderId}/status`, { status }),
}
