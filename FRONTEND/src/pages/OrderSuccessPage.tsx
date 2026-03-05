import { useParams, Link } from 'react-router-dom'
import { useQuery } from 'react-query'
import { orderApi } from '@/api'
import LoadingSpinner from '@/components/ui/LoadingSpinner'
import Button from '@/components/ui/Button'
import { formatCurrency } from '@/utils/format'

const OrderSuccessPage = () => {
  const { orderCode } = useParams<{ orderCode: string }>()

  const { data, isLoading } = useQuery({
    queryKey: ['order', orderCode],
    queryFn: () => orderApi.getOrderByCode(1, orderCode!), // TODO: get user ID from auth
    enabled: !!orderCode,
  })

  if (isLoading) return <div className="flex justify-center py-20"><LoadingSpinner size="lg" /></div>

  const order = data?.data

  return (
    <div className="max-w-3xl mx-auto px-4 py-16 text-center">
      <div className="mb-6">
        <svg className="w-16 h-16 text-green-500 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
        </svg>
      </div>

      <h1 className="text-3xl font-bold text-gray-900 mb-4">Đặt hàng thành công!</h1>
      <p className="text-gray-600 mb-6">
        Cảm ơn bạn đã đặt hàng. Mã đơn hàng của bạn là:
      </p>
      <p className="text-2xl font-mono font-bold text-primary-600 mb-8">{orderCode}</p>

      {order && (
        <div className="bg-white rounded-lg shadow border p-6 text-left mb-8">
          <h2 className="text-lg font-semibold mb-4">Chi tiết đơn hàng</h2>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-gray-600">Trạng thái</span>
              <span>{order.status}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Tổng tiền</span>
              <span className="font-bold">{formatCurrency(order.total)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Phương thức thanh toán</span>
              <span>{order.payment_method}</span>
            </div>
          </div>
        </div>
      )}

      <div className="flex justify-center gap-4">
        <Link to="/my-orders">
          <Button variant="outline">Xem đơn hàng</Button>
        </Link>
        <Link to="/products">
          <Button>Tiếp tục mua sắm</Button>
        </Link>
      </div>
    </div>
  )
}

export default OrderSuccessPage
