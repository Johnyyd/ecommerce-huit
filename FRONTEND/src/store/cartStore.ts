import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { CartDto, CartItemDto } from '@/types'
import { cartApi } from '@/api'
import { toast } from 'react-hot-toast'

interface CartStore {
  cart: CartDto | null
  loading: boolean
  error: string | null
  fetchCart: (userId: number) => Promise<void>
  addItem: (userId: number, variantId: number, quantity: number) => Promise<void>
  updateItem: (userId: number, itemId: number, quantity: number) => Promise<void>
  removeItem: (userId: number, itemId: number) => Promise<void>
  applyVoucher: (userId: number, code: string) => Promise<void>
  clearCart: (userId: number) => Promise<void>
}

const defaultCart: CartDto = {
  id: 0,
  items: [],
  subtotal: 0,
  discount: 0,
  total: 0,
  appliedVoucher: null,
}

export const useCartStore = create<CartStore>()(
  persist(
    (set, get) => ({
      cart: defaultCart,
      loading: false,
      error: null,

      fetchCart: async (userId: number) => {
        set({ loading: true, error: null })
        try {
          const cart = await cartApi.getCart(userId)
          set({ cart, loading: false })
          console.log('fetchCart result:', cart)
        } catch (error: any) {
          set({ error: error.message, loading: false })
          toast.error('Không thể tải giỏ hàng')
        }
      },

      addItem: async (userId: number, variantId: number, quantity: number) => {
        set({ loading: true, error: null })
        try {
          const cart = await cartApi.addItem(userId, { variantId, quantity })
          set({ cart, loading: false })
          console.log('addItem result:', cart)
        } catch (error: any) {
          set({ error: error.message, loading: false })
          toast.error(error.response?.data?.message || 'Không thể thêm sản phẩm')
        }
      },

      updateItem: async (userId: number, itemId: number, quantity: number) => {
        set({ loading: true, error: null })
        try {
          const cart = await cartApi.updateItem(userId, itemId, { quantity })
          set({ cart, loading: false })
        } catch (error: any) {
          set({ error: error.message, loading: false })
          toast.error(error.response?.data?.message || 'Không thể cập nhật')
        }
      },

      removeItem: async (userId: number, itemId: number) => {
        set({ loading: true, error: null })
        try {
          const cart = await cartApi.removeItem(userId, itemId)
          set({ cart, loading: false })
        } catch (error: any) {
          set({ error: error.message, loading: false })
          toast.error('Không thể xóa sản phẩm')
        }
      },

      applyVoucher: async (userId: number, code: string) => {
        set({ loading: true, error: null })
        try {
          const cart = await cartApi.applyVoucher(userId, code)
          set({ cart, loading: false })
        } catch (error: any) {
          set({ error: error.message, loading: false })
          toast.error(error.response?.data?.message || 'Voucher không hợp lệ')
        }
      },

      clearCart: async (userId: number) => {
        set({ loading: true, error: null })
        try {
          const cart = await cartApi.clearCart(userId)
          set({ cart, loading: false })
        } catch (error: any) {
          set({ error: error.message, loading: false })
          toast.error('Không thể xóa giỏ hàng')
        }
      },
    }),
    {
      name: 'cart-storage',
    }
  )
)
