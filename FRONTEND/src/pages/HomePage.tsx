import { useEffect, useState } from 'react'
import { useCartStore } from '@/store/cartStore'
import { useAuthStore } from '@/store/authStore'
import ProductCard from '@/components/product/ProductCard'
import type { ProductListDto } from '@/types'
import { productApi } from '@/api'
import { toast } from 'react-hot-toast'

const HomePage = () => {
  const { user } = useAuthStore()
  const { addItem } = useCartStore()
  const [products, setProducts] = useState<ProductListDto[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const data = await productApi.getProducts({ page: 1, pageSize: 8 })
        setProducts(data || [])
      } catch (error) {
        toast.error('Không thể tải sản phẩm')
        setProducts([])
      } finally {
        setLoading(false)
      }
    }

    fetchProducts()
  }, [])

  const handleAddToCart = async (productId: number) => {
    if (!user) {
      toast.info('Vui lòng đăng nhập để thêm vào giỏ hàng')
      return
    }
    try {
      // Fetch full product to get variant ID
      const product = await productApi.getProductById(productId)
      const variant = product.variants?.[0]
      if (!variant) {
        toast.error('Sản phẩm chưa có biến thể')
        return
      }
      await addItem(user.id, variant.id, 1)
      toast.success('Đã thêm vào giỏ hàng')
    } catch (error) {
      toast.error('Không thể thêm sản phẩm')
    }
  }

  return (
    <div className="bg-gray-50">
      {/* Hero */}
      <section className="bg-primary-600 text-white py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 text-center">
          <h1 className="text-4xl md:text-5xl font-bold mb-4">Chào mừng đến với HUIT Shop</h1>
          <p className="text-xl text-primary-100 mb-8">
            Mua sắm dễ dàng, thanh toán an toàn, giao hàng nhanh chóng
          </p>
          <a
            href="/products"
            className="inline-block bg-white text-primary-600 px-8 py-3 rounded-lg font-semibold hover:bg-gray-100 transition"
          >
            Xem sản phẩm
          </a>
        </div>
      </section>

      {/* Featured Products */}
      <section className="py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between mb-8">
            <h2 className="text-2xl font-bold text-gray-900">Sản phẩm nổi bật</h2>
            <a
              href="/products"
              className="text-primary-600 hover:text-primary-700 font-medium"
            >
              Xem tất cả →
            </a>
          </div>

          {loading ? (
            <div className="flex justify-center items-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
              {products.map((product) => (
                <ProductCard
                  key={product.id}
                  id={product.id}
                  name={product.name}
                  price={product.priceFrom}
                  imageUrl={product.thumbnailUrl || 'https://via.placeholder.com/300'}
                  brand={product.brand?.name}
                  category={product.category?.name}
                  onAddToCart={handleAddToCart}
                />
              ))}
            </div>
          )}
        </div>
      </section>
    </div>
  )
}

export default HomePage
