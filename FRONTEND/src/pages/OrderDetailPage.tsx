import { useParams, Link } from 'react-router-dom'
import { useQuery } from 'react-query'
import { orderApi } from '@/api'
import LoadingSpinner from '@/components/ui/LoadingSpinner'
import { formatCurrency } from '@/utils/format'

const OrderDetailPage = () => {
  const { orderCode } = useParams<{ orderCode: string }>()
  const { user } = useAuthStore()

  const { data, isLoading, error } = useQuery({
    queryKey: ['order', orderCode],
    queryFn: () => orderApi.getOrderByCode(user!.id, orderCode!),
    enabled: !!user && !!orderCode,
  })

  if (!user) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-16 text-center">
        <h2 className="text-2xl font-bold mb-4">Vui lòng đăng nhập</h2>
      </div>
    )
  }

  if (isLoading) return <div className="flex justify-center py-20"><LoadingSpinner size="lg" /></div>
  if (error) return <div className="text-center py-12 text-red-600">Không tìm thấy đơn hàng</div>

  const order = data?.data

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-6">
        <Link to="/my-orders" className="text-primary-600 hover:underline">
          ← Quay lại danh sách đơn hàng
        </Link>
      </div>

      <div className="bg-white rounded-lg shadow border p-6">
        <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Đơn hàng {order?.code}</h1>
            <p className="text-gray-500">Ngày đặt: {new Date(order?.created_at).toLocaleDateString('vi-VN')}</p>
          </div>
          <div className="mt-4 md:mt-0">
            <span className={`inline-block px-4 py-2 rounded-full text-sm font-semibold ${
              order?.status === 'COMPLETED' ? 'bg-green-100 text-green-800' :
              order?.status === 'CANCELLED' ? 'bg-red-100 text-red-800' :
              order?.status === 'PENDING' ? 'bg-yellow-100 text-yellow-800' :
              order?.status === 'SHIPPING' ? 'bg-blue-100 text-blue-800' :
              'bg-gray-100 text-gray-800'
            }`}>
              {order?.status}
            </span>
          </div>
        </div>

        {/* Items */}
        <div className="border-t pt-6 mb-6">
          <h2 className="text-lg font-semibold mb-4">Sản phẩm</h2>
          <div className="space-y-4">
            {order?.items.map((item: any) => (
              <div key={item.id} className="flex justify-between items-center">
                <div>
                  <p className="font-medium">{item.product_name}</p>
                  <p className="text-sm text-gray-500">{item.sku}</p>
                  {item.serial_numbers && item.serial_numbers.length > 0 && (
                    <p className="text-xs text-gray-400">Serial: {item.serial_numbers.join(', ')}</p>
                  )}
                </div>
                <div className="text-right">
                  <p className="font-medium">{formatCurrency(item.unit_price)} x {item.quantity}</p>
                  <p className="text-red-600 font-bold">{formatCurrency(item.total_price)}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Summary */}
        <div className="border-t pt-6 mb-6 space-y-2">
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Tạm tính</span>
            <span>{formatCurrency(order?.subtotal)}</span>
          </div>
          {order?.discount > 0 && (
            <div className="flex justify-between text-sm text-red-600">
              <span>Giảm giá</span>
              <span>-{formatCurrency(order?.discount)}</span>
            </div>
          )}
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Phí vận chuyển</span>
            <span>{formatCurrency(order?.shipping_fee)}</span>
          </div>
          <div className="flex justify-between text-lg font-bold pt-2 border-t">
            <span>Tổng cộng</span>
            <span className="text-red-600">{formatCurrency(order?.total)}</span>
          </div>
        </div>

        {/* Address & Payment */}
        <div className="border-t pt-6 grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <h3 className="font-semibold mb-2">Địa chỉ giao hàng</h3>
            {order?.shipping_address_json ? (
              <pre className="text-sm text-gray-600 whitespace-pre-wrap">
                {JSON.stringify(JSON.parse(order.shipping_address_json), null, 2)}
              </pre>
            ) : (
              <p className="text-sm text-gray-500">Không có thông tin</p>
            )}
          </div>
          <div>
            <h3 className="font-semibold mb-2">Thanh toán</h3>
            <p className="text-sm text-gray-600">{order?.payment_method}</p>
            <p className="text-sm text-gray-600">Trạng thái: {order?.payment_status}</p>
          </div>
        </div>

        {/* Status history */}
        {order?.status_history && order.status_history.length > 0 && (
          <div className="border-t pt-6 mt-6">
            <h3 className="font-semibold mb-4">Lịch sử trạng thái</h3>
            <ul className="space-y-2">
              {order.status_history.map((history: any) => (
                <li key={history.id} className="text-sm text-gray-600">
                  <span className="font-medium">{history.status}</span>
                  {history.note && <span> - {history.note}</span>}
                  <span className="text-xs text-gray-400 ml-2">
                    {new Date(history.created_at).toLocaleString('vi-VN')}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Cancel button (if pending) */}
        {order?.status === 'PENDING' && (
          <div className="border-t pt-6 mt-6 text-center">
            <Button variant="danger">Hủy đơn hàng</Button>
          </div>
        )}
      </div>
    </div>
  )
}

export default OrderDetailPage
