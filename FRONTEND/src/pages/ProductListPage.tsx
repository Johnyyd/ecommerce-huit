import { useState } from 'react'
import { useQuery } from 'react-query'
import { productApi } from '@/api'
import ProductCard from '@/components/product/ProductCard'
import { useCartStore } from '@/store/cartStore'
import { useAuthStore } from '@/store/authStore'
import { toast } from 'react-hot-toast'
import LoadingSpinner from '@/components/ui/LoadingSpinner'
import Button from '@/components/ui/Button'
import Pagination from '@/components/ui/Pagination'
import { cn } from '@/utils/format'

const ProductListPage = () => {
  const { user } = useAuthStore()
  const { addItem } = useCartStore()
  const [page, setPage] = useState(1)
  const [filters, setFilters] = useState({
    category: '',
    brand: '',
    minPrice: '',
    maxPrice: '',
    search: '',
  })

  const { data, isLoading, error } = useQuery({
    queryKey: ['products', page, filters],
    queryFn: () => productApi.getProducts({ page, pageSize: 12, ...filters }),
  })

  const handleAddToCart = (variantId: number) => {
    if (!user) {
      toast.info('Vui lòng đăng nhập để thêm vào giỏ hàng')
      return
    }
    addItem(user.id, variantId, 1)
  }

  const products = data?.data || []
  const pagination = data?.pagination

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Tất cả sản phẩm</h1>

      <div className="flex flex-col lg:flex-row gap-8">
        {/* Filters sidebar */}
        <aside className="lg:w-64 space-y-6">
          <div className="bg-white p-4 rounded-lg shadow border">
            <h3 className="font-semibold mb-4">Tìm kiếm</h3>
            <input
              type="text"
              placeholder="Tên sản phẩm..."
              className="w-full border rounded-lg px-3 py-2"
              value={filters.search}
              onChange={(e) => setFilters({ ...filters, search: e.target.value })}
            />
          </div>

          <div className="bg-white p-4 rounded-lg shadow border">
            <h3 className="font-semibold mb-4">Danh mục</h3>
            <select
              className="w-full border rounded-lg px-3 py-2"
              value={filters.category}
              onChange={(e) => setFilters({ ...filters, category: e.target.value })}
            >
              <option value="">Tất cả danh mục</option>
              {/* Populate from API */}
            </select>
          </div>

          <div className="bg-white p-4 rounded-lg shadow border">
            <h3 className="font-semibold mb-4">Khoảng giá</h3>
            <div className="flex gap-2">
              <input
                type="number"
                placeholder="Min"
                className="w-1/2 border rounded px-2 py-1"
                value={filters.minPrice}
                onChange={(e) => setFilters({ ...filters, minPrice: e.target.value })}
              />
              <input
                type="number"
                placeholder="Max"
                className="w-1/2 border rounded px-2 py-1"
                value={filters.maxPrice}
                onChange={(e) => setFilters({ ...filters, maxPrice: e.target.value })}
              />
            </div>
          </div>
        </aside>

        {/* Product grid */}
        <div className="flex-1">
          {isLoading ? (
            <div className="flex justify-center items-center py-12">
              <LoadingSpinner size="lg" />
            </div>
          ) : error ? (
            <div className="text-red-600 text-center py-12">Không thể tải sản phẩm</div>
          ) : (
            <>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
                {products.map((product) => (
                  <ProductCard
                    key={product.id}
                    id={product.id}
                    name={product.name}
                    price={product.price}
                    imageUrl={product.thumbnail_url || ''}
                    brand={product.brand_name}
                    category={product.category_name}
                    onAddToCart={handleAddToCart}
                  />
                ))}
              </div>

              {pagination && (
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
      </div>
    </div>
  )
}

export default ProductListPage
