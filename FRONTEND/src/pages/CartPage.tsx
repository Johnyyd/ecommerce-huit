import { useEffect } from 'react'
import { useCartStore } from '@/store/cartStore'
import { useAuthStore } from '@/store/authStore'
import { formatCurrency } from '@/utils/format'
import { Link } from 'react-router-dom'
import Button from '@/components/ui/Button'
import LoadingSpinner from '@/components/ui/LoadingSpinner'
import { toast } from 'react-hot-toast'

const CartPage = () => {
  const { user } = useAuthStore()
  const { cart, loading, fetchCart, removeItem, updateItem, clearCart } = useCartStore()

  useEffect(() => {
    if (user) {
      fetchCart(user.id)
    }
  }, [user])

  const handleUpdateQuantity = async (itemId: number, newQuantity: number) => {
    if (!user) return
    await updateItem(user.id, itemId, newQuantity)
  }

  const handleRemove = async (itemId: number) => {
    if (!user) return
    await removeItem(user.id, itemId)
  }

  const handleApplyVoucher = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    if (!user) return
    const form = e.currentTarget
    const input = form.elements.namedItem('voucher') as HTMLInputElement
    await useCartStore.getState().applyVoucher(user.id, input.value)
    form.reset()
  }

  if (!user) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-16 text-center">
        <h2 className="text-2xl font-bold mb-4">Vui lòng đăng nhập</h2>
        <Link to="/login" className="text-primary-600 hover:underline">
          Đăng nhập
        </Link>
      </div>
    )
  }

  if (loading && !cart) {
    return (
      <div className="flex justify-center items-center py-20">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Giỏ hàng</h1>

      {(cart?.items || []).length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 mb-4">Giỏ hàng trống</p>
          <Link to="/products">
            <Button>Tiếp tục mua sắm</Button>
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Cart items */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-lg shadow border">
              {(cart?.items || []).map((item, index) => (
                <div
                  key={item.id}
                  className={`flex items-center gap-4 p-4 ${
                    index !== ((cart?.items || []).length - 1) ? 'border-b' : ''
                  }`}
                >
                  <img
                    src={item.variant.thumbnail_url || 'https://via.placeholder.com/100'}
                    alt=""
                    className="w-20 h-20 object-cover rounded"
                  />
                  <div className="flex-1">
                    <h3 className="font-semibold text-gray-900">{item.variant.sku}</h3>
                    <p className="text-gray-600 text-sm">{item.variant.variant_name}</p>
                    <p className="text-red-600 font-bold mt-1">
                      {formatCurrency(item.variant.price)}
                    </p>
                  </div>

                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => handleUpdateQuantity(item.id, item.quantity - 1)}
                      className="w-8 h-8 flex items-center justify-center border rounded hover:bg-gray-50"
                    >
                      -
                    </button>
                    <span className="w-8 text-center">{item.quantity}</span>
                    <button
                      onClick={() => handleUpdateQuantity(item.id, item.quantity + 1)}
                      className="w-8 h-8 flex items-center justify-center border rounded hover:bg-gray-50"
                    >
                      +
                    </button>
                  </div>

                  <div className="text-right">
                    <p className="font-semibold">{formatCurrency(item.line_total)}</p>
                    <button
                      onClick={() => handleRemove(item.id)}
                      className="text-red-500 text-sm hover:underline mt-1"
                    >
                      Xóa
                    </button>
                  </div>
                </div>
              ))}
            </div>

            <div className="mt-4 flex justify-end">
              <Button variant="ghost" onClick={() => clearCart(user!.id)}>
                Xóa tất cả
              </Button>
            </div>
          </div>

          {/* Summary */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow border p-6 sticky top-24">
              <h2 className="text-lg font-semibold mb-4">Tổng đơn hàng</h2>

              <div className="space-y-3 text-sm">
                <div className="flex justify-between">
                  <span>Tạm tính</span>
                  <span>{formatCurrency(cart.subtotal)}</span>
                </div>
                <div className="flex justify-between">
                  <span>Giảm giá</span>
                  <span className="text-red-600">-{formatCurrency(cart.discount)}</span>
                </div>
                <div className="flex justify-between pt-3 border-t font-semibold text-lg">
                  <span>Tổng cộng</span>
                  <span>{formatCurrency(cart.total)}</span>
                </div>
              </div>

              {/* Voucher */}
              <form onSubmit={handleApplyVoucher} className="mt-6">
                <label className="block text-sm font-medium mb-2">Mã giảm giá</label>
                <div className="flex gap-2">
                  <input
                    name="voucher"
                    type="text"
                    placeholder="Nhập mã"
                    className="flex-1 border rounded-lg px-3 py-2"
                  />
                  <Button type="submit" variant="outline">
                    Áp dụng
                  </Button>
                </div>
              </form>

              <div className="mt-6">
                <Link to="/checkout">
                  <Button className="w-full" size="lg">
                    Tiến hành thanh toán
                  </Button>
                </Link>
              </div>

              <p className="text-xs text-gray-500 text-center mt-4">
                Phí vận chuyển sẽ được tính khi chọn địa chỉ
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default CartPage
