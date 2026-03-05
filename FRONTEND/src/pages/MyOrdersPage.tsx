import { useState } from 'react'
import { useQuery } from 'react-query'
import { orderApi } from '@/api'
import { useAuthStore } from '@/store/authStore'
import LoadingSpinner from '@/components/ui/LoadingSpinner'
import Button from '@/components/ui/Button'
import { formatCurrency } from '@/utils/format'
import { Link } from 'react-router-dom'

const MyOrdersPage = () => {
  const { user } = useAuthStore()
  const [page, setPage] = useState(1)

  const { data, isLoading, error } = useQuery({
    queryKey: ['orders', user?.id, page],
    queryFn: () => orderApi.getOrders(user!.id, { page, pageSize: 10 }),
    enabled: !!user,
  })

  if (!user) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-16 text-center">
        <h2 className="text-2xl font-bold mb-4">Vui lòng đăng nhập</h2>
      </div>
    )
  }

  if (isLoading) return <div className="flex justify-center py-20"><LoadingSpinner size="lg" /></div>
  if (error) return <div className="text-center py-12 text-red-600">Không thể tải đơn hàng</div>

  const orders = data?.data || []
  const pagination = data?.pagination

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Đơn hàng của tôi</h1>

      {orders.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 mb-4">Chưa có đơn hàng nào</p>
          <Link to="/products" className="text-primary-600 hover:underline">
            Tiếp tục mua sắm
          </Link>
        </div>
      ) : (
        <>
          <div className="space-y-4">
            {orders.map((order: any) => (
              <div key={order.id} className="bg-white rounded-lg shadow border p-6">
                <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-4">
                  <div>
                    <p className="font-semibold text-gray-900">Mã đơn: {order.code}</p>
                    <p className="text-sm text-gray-500">Ngày: {new Date(order.created_at).toLocaleDateString('vi-VN')}</p>
                  </div>
                  <div className="mt-2 md:mt-0">
                    <span className={`inline-block px-3 py-1 rounded-full text-sm font-medium ${
                      order.status === 'COMPLETED' ? 'bg-green-100 text-green-800' :
                      order.status === 'CANCELLED' ? 'bg-red-100 text-red-800' :
                      order.status === 'PENDING' ? 'bg-yellow-100 text-yellow-800' :
                      'bg-blue-100 text-blue-800'
                    }`}>
                      {order.status}
                    </span>
                  </div>
                </div>

                <div className="border-t pt-4">
                  {(order.items || []).slice(0, 3).map((item: any, idx: number) => (
                    <div key={idx} className="flex justify-between text-sm mb-2">
                      <span>{item.product_name} x{item.quantity}</span>
                      <span>{formatCurrency(item.total_price)}</span>
                    </div>
                  ))}
                  {(order.items || []).length > 3 && (
                    <p className="text-sm text-gray-500">+{(order.items || []).length - 3} sản phẩm khác</p>
                  )}
                </div>

                <div className="border-t mt-4 pt-4 flex justify-between items-center">
                  <div>
                    <p className="text-sm text-gray-600">Tổng cộng</p>
                    <p className="text-xl font-bold text-red-600">{formatCurrency(order.total)}</p>
                  </div>
                  <div className="flex gap-2">
                    <Link to={`/my-orders/${order.code}`}>
                      <Button variant="outline" size="sm">Chi tiết</Button>
                    </Link>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {pagination && pagination.totalPages > 1 && (
            <div className="mt-8">
              <Pagination
                currentPage={pagination.page}
                totalPages={pagination.totalPages}
                onPageChange={setPage}
              />
            </div>
          )}
        </>
      )}
    </div>
  )
}

export default MyOrdersPage
