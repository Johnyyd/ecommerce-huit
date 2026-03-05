import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'

try {
  console.log('🚀 Starting app...')
  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <App />
    </StrictMode>,
  )
  console.log('✅ App rendered')
} catch (error) {
  console.error('❌ Failed to render app:', error)
  // Show error on page
  document.body.innerHTML = `<div style="color: red; padding: 20px;">
    <h1>Lỗi khởi động ứng dụng</h1>
    <pre>${error}</pre>
  </div>`
}
