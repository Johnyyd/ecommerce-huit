// Product types
export interface ProductVariantDto {
  id: number
  sku: string
  variantName?: string
  price: number
  originalPrice?: number
  thumbnailUrl?: string
  quantityAvailable: number
}

export interface ProductListDto {
  id: number
  name: string
  slug: string
  brand?: BrandDto
  category?: CategoryDto
  variantName?: string
  price: number
  priceFrom?: number
  priceTo?: number
  originalPrice?: number
  thumbnailUrl?: string
  isFeatured?: boolean
}

export interface ProductDetailDto {
  id: number
  name: string
  slug: string
  description?: string
  brand?: BrandDto
  category?: CategoryDto
  variants: ProductVariantDto[]
  images: string[]
  isFeatured?: boolean
  createdAt: string
}

export interface CategoryDto {
  id: number
  name: string
  slug: string
  parentId?: number
  isActive: boolean
}

export interface BrandDto {
  id: number
  name: string
  slug: string
}

export interface ProductQueryParams {
  page?: number
  pageSize?: number
  category?: string
  brand?: string
  minPrice?: number
  maxPrice?: number
  search?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// Cart types
export interface CartItemDto {
  id: number
  variant: ProductVariantDto
  quantity: number
  lineTotal: number
}

export interface CartDto {
  id: number
  items: CartItemDto[]
  subtotal: number
  discount: number
  total: number
  appliedVoucher?: VoucherDto | null
}

export interface AddCartItemRequest {
  variantId: number
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}

// Order types
export interface OrderItemDto {
  id: number
  productName: string
  sku: string
  quantity: number
  unitPrice: number
  totalPrice: number
  serialNumbers?: string[]
}

export interface OrderStatusHistoryDto {
  id: number
  status: string
  note?: string
  createdAt: string
}

export interface OrderResponseDto {
  id: number
  code: string
  subtotal: number
  discount: number
  shippingFee: number
  total: number
  paymentMethod: string
  paymentStatus: string
  status: string
  shippingAddressJson?: string
  note?: string
  createdAt: string
  items: OrderItemDto[]
  statusHistory: OrderStatusHistoryDto[]
}

export interface CreateOrderRequest {
  shippingAddressJson: string
  paymentMethod: string
  note?: string
}

// Auth types
export interface RegisterDto {
  full_name: string
  email: string
  phone: string
  password: string
}

export interface LoginDto {
  email: string
  password: string
}

export interface AuthResponseDto {
  id: number
  email: string
  full_name: string
  role: string
  access_token: string
  refresh_token: string
}

export interface UserDto {
  id: number
  email: string
  full_name: string
  phone?: string
  role: string
  status: string
}

// Voucher types
export interface VoucherDto {
  id: number
  code: string
  discountType: 'PERCENT' | 'FIXED'
  discountValue: number
  maxDiscountAmount?: number
  minOrderAmount?: number
  expiresAt?: string
}

export interface ApplyVoucherRequest {
  code: string
}

// Common types
export interface PaginationDto {
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasNext: boolean
  hasPrev: boolean
}

export interface ApiResponse<T> {
  data: T
  pagination?: PaginationDto
  message?: string
}

// User address
export interface AddressDto {
  id: number
  label: string
  receiverName: string
  receiverPhone: string
  province: string
  district: string
  ward: string
  streetAddress: string
  isDefault: boolean
}

// Admin/Inventory
export interface InventoryDto {
  productName: string
  variantSku: string
  warehouseName: string
  quantityOnHand: number
  quantityReserved: number
}

// Payment
export interface PaymentDto {
  id: number
  orderId: number
  method: string
  amount: number
  status: string
  paidAt?: string
  transactionId?: string
}
