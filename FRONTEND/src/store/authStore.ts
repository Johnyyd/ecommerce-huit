import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { UserDto, AuthResponseDto } from '@/types'
import { authApi } from '@/api'
import { toast } from 'react-hot-toast'

interface AuthStore {
  user: UserDto | null
  token: string | null
  isAuthenticated: boolean
  loading: boolean
  error: string | null
  login: (email: string, password: string) => Promise<boolean>
  register: (data: {
    full_name: string
    email: string
    phone: string
    password: string
  }) => Promise<boolean>
  logout: () => void
  setUser: (user: UserDto | null) => void
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      loading: false,
      error: null,

      login: async (email: string, password: string) => {
        set({ loading: true, error: null })
        try {
          const response = await authApi.login({ email, password })
          set({
            user: {
              id: response.id,
              email: response.email,
              full_name: response.full_name,
              role: response.role,
              status: 'ACTIVE',
            },
            token: response.access_token,
            isAuthenticated: true,
            loading: false,
          })
          localStorage.setItem('access_token', response.access_token)
          localStorage.setItem('refresh_token', response.refresh_token)
          return true
        } catch (error: any) {
          set({ error: error.message, loading: false })
          return false
        }
      },

      register: async (data) => {
        set({ loading: true, error: null })
        try {
          const response = await authApi.register(data)
          set({
            user: {
              id: response.id,
              email: response.email,
              full_name: response.full_name,
              role: response.role,
              status: 'ACTIVE',
            },
            token: response.access_token,
            isAuthenticated: true,
            loading: false,
          })
          localStorage.setItem('access_token', response.access_token)
          localStorage.setItem('refresh_token', response.refresh_token)
          return true
        } catch (error: any) {
          set({ error: error.message, loading: false })
          return false
        }
      },

      logout: () => {
        localStorage.removeItem('access_token')
        localStorage.removeItem('refresh_token')
        toast.success('Đã đăng xuất')
        set({
          user: null,
          token: null,
          isAuthenticated: false,
          loading: false,
        })
      },

      setUser: (user) => {
        set({ user, isAuthenticated: !!user })
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ token: state.token, user: state.user, isAuthenticated: state.isAuthenticated }),
    }
  )
)
