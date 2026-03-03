import { useParams, Link } from 'react-router-dom'
import { useQuery } from 'react-query'
import { productApi } from '@/api'
import { useCartStore } from '@/store/cartStore'
import { useAuthStore } from '@/store/authStore'
import { toast } from 'react-hot-toast'
import LoadingSpinner from '@/components/ui/LoadingSpinner'
import Button from '@/components/ui/Button'
import { formatCurrency } from '@/utils/format'

const ProductDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const { user } = useAuthStore()
  const { addItem } = useCartStore()
  const [selectedVariantId, setSelectedVariantId] = useState<number | null>(null)
  const [quantity, setQuantity] = useState(1)

  const { data, isLoading, error } = useQuery({
    queryKey: ['product', id],
    queryFn: () => productApi.getProductById(Number(id)),
    enabled: !!id,
  })

  const product = data?.data

  const handleAddToCart = () => {
    if (!user) {
      toast.info('Vui lòng đăng nhập để thêm vào giỏ hàng')
      return
    }
    if (!selectedVariantId) {
      toast.error('Vui lòng chọn biến thể')
      return
    }
    addItem(user.id, selectedVariantId, quantity)
  }

  if (isLoading) return <div className="flex justify-center py-12"><LoadingSpinner size="lg" /></div>
  if (error || !product) return <div className="text-center py-12 text-red-600">Không tìm thấy sản phẩm</div>

  const mainImage = product.variants[0]?.thumbnail_url || 'https://via.placeholder.com/600'

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Images */}
        <div className="bg-white p-4 rounded-lg shadow">
          <img
            src={mainImage}
            alt={product.name}
            className="w-full aspect-square object-cover rounded-lg"
          />
          {/* Thumbnails - optional */}
        </div>

        {/* Info */}
        <div>
          <nav className="text-sm mb-4">
            <Link to="/products" className="text-primary-600 hover:underline">
              Sản phẩm
            </Link>
            <span className="mx-2 text-gray-400">/</span>
            <span className="text-gray-600">{product.name}</span>
          </nav>

          <h1 className="text-3xl font-bold text-gray-900 mb-4">{product.name}</h1>

          {product.brand_name && (
            <p className="text-gray-600 mb-2">Thương hiệu: {product.brand_name}</p>
          )}

          <div className="flex items-center gap-4 mb-6">
            <span className="text-4xl font-bold text-red-600">
              {formatCurrency(product.variants[0]?.price || 0)}
            </span>
            {product.variants[0]?.original_price && (
              <span className="text-xl text-gray-400 line-through">
                {formatCurrency(product.variants[0].original_price)}
              </span>
            )}
          </div>

          <p className="text-gray-700 mb-6">{product.description || 'Chưa có mô tả.'}</p>

          {/* Variant selection */}
          {product.variants.length > 1 && (
            <div className="mb-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">Biến thể</label>
              <div className="flex flex-wrap gap-2">
                {product.variants.map((variant) => (
                  <button
                    key={variant.id}
                    onClick={() => setSelectedVariantId(variant.id)}
                    className={`px-4 py-2 border rounded-lg ${
                      selectedVariantId === variant.id
                        ? 'border-primary-600 bg-primary-50 text-primary-700'
                        : 'border-gray-300 hover:border-primary-300'
                    }`}
                  >
                    {variant.variant_name || variant.sku}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Quantity */}
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">Số lượng</label>
            <div className="flex items-center gap-3">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setQuantity(Math.max(1, quantity - 1))}
              >
                -
              </Button>
              <span className="text-lg font-medium w-8 text-center">{quantity}</span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setQuantity(quantity + 1)}
              >
                +
              </Button>
            </div>
          </div>

          {/* Add to cart */}
          <Button onClick={handleAddToCart} size="lg" className="w-full mb-3">
            Thêm vào giỏ
          </Button>

          <Button variant="outline" size="lg" className="w-full">
            Mua ngay
          </Button>

          {/* Stock info */}
          <p className="mt-4 text-sm text-gray-500">
            Còn lại: {product.variants[0]?.quantity_available || 0} sản phẩm
          </p>
        </div>
      </div>
    </div>
  )
}

export default ProductDetailPage
