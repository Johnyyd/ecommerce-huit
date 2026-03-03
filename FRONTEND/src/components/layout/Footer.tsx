import { Link } from 'react-router-dom'

const Footer = () => {
  return (
    <footer className="bg-white border-t border-gray-200">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {/* Company */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">ECommerce HUIT</h3>
            <p className="text-gray-600 text-sm leading-relaxed">
              Hệ thống bán hàng điện tử hiện đại, được phát triển với công nghệ mới nhất.
            </p>
          </div>

          {/* Links */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Liên kết</h3>
            <ul className="space-y-2 text-sm text-gray-600">
              <li>
                <Link to="/products" className="hover:text-primary-600 transition">
                  Sản phẩm
                </Link>
              </li>
              <li>
                <Link to="/cart" className="hover:text-primary-600 transition">
                  Giỏ hàng
                </Link>
              </li>
              <li>
                <Link to="/my-orders" className="hover:text-primary-600 transition">
                  Đơn hàng của tôi
                </Link>
              </li>
            </ul>
          </div>

          {/* Contact */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Liên hệ</h3>
            <ul className="space-y-2 text-sm text-gray-600">
              <li>Email: support@huit.edu.vn</li>
              <li>Hotline: 1900 1234</li>
              <li>Địa chỉ: Đại học Bách khoa Hà Nội</li>
            </ul>
          </div>
        </div>

        <div className="mt-8 border-t border-gray-200 pt-8 text-center text-sm text-gray-500">
          <p>&copy; {new Date().getFullYear()} ECommerce HUIT. All rights reserved.</p>
        </div>
      </div>
    </footer>
  )
}

export default Footer
