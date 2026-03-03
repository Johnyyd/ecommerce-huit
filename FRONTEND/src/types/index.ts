// Product types
export interface ProductVariantDto {
  id: number
  sku: string
  variant_name?: string
  price: number
  original_price?: number
  thumbnail_url?: string
  quantity_available: number
}

export interface ProductListDto {
  id: number
  name: string
  slug: string
  brand_name?: string
  category_name?: string
  variant_name?: string
  price: number
  original_price?: number
  thumbnail_url?: string
  is_featured?: boolean
}

export interface ProductDetailDto {
  id: number
  name: string
  slug: string
  description?: string
  brand_name?: string
  category_name?: string
  variants: ProductVariantDto[]
  images: string[]
  is_featured?: boolean
  created_at: string
}

export interface CategoryDto {
  id: number
  name: string
  slug: string
  parent_id?: number
  is_active: boolean
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
  line_total: number
}

export interface CartDto {
  id: number
  items: CartItemDto[]
  subtotal: number
  discount: number
  total: number
  applied_voucher?: VoucherDto | null
}

export interface AddCartItemRequest {
  variant_id: number
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}

// Order types
export interface OrderItemDto {
  id: number
  product_name: string
  sku: string
  quantity: number
  unit_price: number
  total_price: number
  serial_numbers?: string[]
}

export interface OrderStatusHistoryDto {
  id: number
  status: string
  note?: string
  created_at: string
}

export interface OrderResponseDto {
  id: number
  code: string
  subtotal: number
  discount: number
  shipping_fee: number
  total: number
  payment_method: string
  payment_status: string
  status: string
  shipping_address_json?: string
  note?: string
  created_at: string
  items: OrderItemDto[]
  status_history: OrderStatusHistoryDto[]
}

export interface CreateOrderRequest {
  shipping_address_json: string
  payment_method: string
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
  discount_type: 'PERCENT' | 'FIXED'
  discount_value: number
  max_discount_amount?: number
  min_order_amount?: number
  expires_at?: string
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
  receiver_name: string
  receiver_phone: string
  province: string
  district: string
  ward: string
  street_address: string
  is_default: boolean
}

// Admin/Inventory
export interface InventoryDto {
  product_name: string
  variant_sku: string
  warehouse_name: string
  quantity_on_hand: number
  quantity_reserved: number
}

// Payment
export interface PaymentDto {
  id: number
  order_id: number
  method: string
  amount: number
  status: string
  paid_at?: string
  transaction_id?: string
}
