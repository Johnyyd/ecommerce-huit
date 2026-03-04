import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '@/store/authStore'
import Button from '@/components/ui/Button'
import Input from '@/components/ui/Input'
import { toast } from 'react-hot-toast'

const RegisterPage = () => {
  const navigate = useNavigate()
  const { register } = useAuthStore()
  const [formData, setFormData] = useState({
    full_name: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: '',
  })
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (formData.password !== formData.confirmPassword) {
      toast.error('Mật khẩu xác nhận không khớp')
      return
    }

    setLoading(true)
    const success = await register({
      full_name: formData.full_name,
      email: formData.email,
      phone: formData.phone,
      password: formData.password,
    })

    if (success) {
      navigate('/')
    } else {
      toast.error('Đăng ký thất bại. Email có thể đã tồn tại.')
    }
    setLoading(false)
  }

  return (
    <div className="min-h-[calc(100vh-200px)] flex items-center justify-center px-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow border p-8">
        <h1 className="text-2xl font-bold text-center mb-6">Đăng ký tài khoản</h1>

        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label="Họ và tên"
            type="text"
            required
            value={formData.full_name}
            onChange={(e) => setFormData({ ...formData, full_name: e.target.value })}
          />
          <Input
            label="Email"
            type="email"
            required
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          />
          <Input
            label="Số điện thoại"
            type="tel"
            required
            value={formData.phone}
            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
          />
          <Input
            label="Mật khẩu"
            type="password"
            required
            minLength={6}
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
          />
          <Input
            label="Xác nhận mật khẩu"
            type="password"
            required
            value={formData.confirmPassword}
            onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
          />

          <Button type="submit" className="w-full" isLoading={loading}>
            Đăng ký
          </Button>
        </form>

        <p className="text-center mt-6 text-sm text-gray-600">
          Đã có tài khoản?{' '}
          <button onClick={() => navigate('/login')} className="text-primary-600 hover:underline">
            Đăng nhập
          </button>
        </p>
      </div>
    </div>
  )
}

export default RegisterPage
