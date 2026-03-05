import { Fragment } from 'react'
import { Disclosure, Menu, Transition } from '@headlessui/react'
import { Menu as MenuIcon, X as XIcon } from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import { toast } from 'react-hot-toast'
import { cn } from '@/utils/format'

const navigation = [
  { name: 'Trang chủ', href: '/' },
  { name: 'Sản phẩm', href: '/products' },
  { name: 'Giỏ hàng', href: '/cart' },
]

const Header = () => {
  const location = useLocation()
  const { user, isAuthenticated, logout } = useAuthStore()

  return (
    <Disclosure as="header" className="bg-white shadow sticky top-0 z-40">
      {({ open }) => (
        <>
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="flex h-16 justify-between items-center">
              {/* Logo */}
              <div className="flex-shrink-0">
                <Link to="/" className="text-2xl font-bold text-primary-600">
                  HUIT Shop
                </Link>
              </div>

              {/* Desktop Navigation */}
              <nav className="hidden md:flex space-x-8">
                {navigation.map((item) => (
                  <Link
                    key={item.name}
                    to={item.href}
                    className={cn(
                      'text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium',
                      location.pathname === item.href && 'text-primary-600 bg-primary-50'
                    )}
                  >
                    {item.name}
                  </Link>
                ))}
              </nav>

              {/* User menu */}
              <div className="flex items-center gap-4">
                {isAuthenticated ? (
                  <Menu as="div" className="relative">
                    <Menu.Button className="flex items-center gap-2 text-gray-700 hover:text-primary-600">
                      <span className="text-sm font-medium">{user?.full_name}</span>
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M19 9l-7 7-7-7"
                        />
                      </svg>
                    </Menu.Button>
                    <Transition
                      as={Fragment}
                      enter="transition ease-out duration-100"
                      enterFrom="transform opacity-0 scale-95"
                      enterTo="transform opacity-100 scale-100"
                      leave="transition ease-in duration-75"
                      leaveFrom="transform opacity-100 scale-100"
                      leaveTo="transform opacity-0 scale-95"
                    >
                      <Menu.Items className="absolute right-0 mt-2 w-48 origin-top-right rounded-lg bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none">
                        <Menu.Item>
                          {({ active }) => (
                            <Link
                              to="/my-orders"
                              className={cn(
                                'block px-4 py-2 text-sm text-gray-700',
                                active && 'bg-gray-100'
                              )}
                            >
                              Đơn hàng của tôi
                            </Link>
                          )}
                        </Menu.Item>
                        <Menu.Item>
                          {({ active }) => (
                            <Link
                              to="/profile"
                              className={cn(
                                'block px-4 py-2 text-sm text-gray-700',
                                active && 'bg-gray-100'
                              )}
                            >
                              Hồ sơ
                            </Link>
                          )}
                        </Menu.Item>
                        <Menu.Item>
                          {({ active }) => (
                            <button
                              onClick={() => {
                                logout()
                                toast.success('Đã đăng xuất')
                              }}
                              className={cn(
                                'block w-full text-left px-4 py-2 text-sm text-red-600',
                                active && 'bg-gray-100'
                              )}
                            >
                              Đăng xuất
                            </button>
                          )}
                        </Menu.Item>
                      </Menu.Items>
                    </Transition>
                  </Menu>
                ) : (
                  <div className="flex gap-2">
                    <Link
                      to="/login"
                      className="text-gray-700 hover:text-primary-600 px-3 py-2 text-sm font-medium"
                    >
                      Đăng nhập
                    </Link>
                    <Link
                      to="/register"
                      className="bg-primary-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-primary-700"
                    >
                      Đăng ký
                    </Link>
                  </div>
                )}
              </div>

              {/* Mobile menu button */}
              <div className="md:hidden">
                <Disclosure.Button className="text-gray-700 hover:text-primary-600">
                  {open ? (
                    <XIcon className="h-6 w-6" />
                  ) : (
                    <MenuIcon className="h-6 w-6" />
                  )}
                </Disclosure.Button>
              </div>
            </div>
          </div>

          {/* Mobile menu */}
          <Disclosure.Panel className="md:hidden">
            <div className="space-y-1 px-4 pb-4 pt-2">
              {navigation.map((item) => (
                <Disclosure.Button
                  key={item.name}
                  as={Link}
                  to={item.href}
                  className={cn(
                    'block rounded-lg px-3 py-2 text-base font-medium',
                    location.pathname === item.href
                      ? 'bg-primary-50 text-primary-600'
                      : 'text-gray-700 hover:bg-gray-50'
                  )}
                >
                  {item.name}
                </Disclosure.Button>
              ))}
            </div>
          </Disclosure.Panel>
        </>
      )}
    </Disclosure>
  )
}

export default Header
