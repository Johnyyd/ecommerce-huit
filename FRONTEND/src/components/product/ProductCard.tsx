import LoadingSpinner from './LoadingSpinner'
import Button from '@/components/ui/Button'

interface ProductCardProps {
  id: number
  name: string
  price: number
  originalPrice?: number
  imageUrl: string
  brand?: string
  category?: string
  onAddToCart?: (productId: number) => void
}

const ProductCard = ({
  id,
  name,
  price,
  originalPrice,
  imageUrl,
  brand,
  category,
  onAddToCart,
}: ProductCardProps) => {
  return (
    <div className="bg-white rounded-lg shadow border border-gray-200 overflow-hidden hover:shadow-lg transition">
      <div className="aspect-square overflow-hidden bg-gray-100">
        <img
          src={imageUrl || 'https://via.placeholder.com/300'}
          alt={name}
          className="w-full h-full object-cover"
          loading="lazy"
        />
      </div>
      <div className="p-4">
        <p className="text-sm text-gray-500 mb-1">{category}</p>
        <h3 className="font-semibold text-gray-900 mb-2 line-clamp-2">{name}</h3>
        <div className="flex items-center gap-2 mb-3">
          <span className="text-xl font-bold text-red-600">{price?.toLocaleString('vi-VN')} ₫</span>
          {originalPrice && originalPrice > price && (
            <span className="text-sm text-gray-400 line-through">
              {originalPrice.toLocaleString('vi-VN')} ₫
            </span>
          )}
        </div>

        {onAddToCart && (
          <Button onClick={() => onAddToCart(id)} className="w-full">
            Thêm vào giỏ
          </Button>
        )}
      </div>
    </div>
  )
}

export default ProductCard
