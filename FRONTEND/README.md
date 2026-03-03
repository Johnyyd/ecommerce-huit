# Frontend - ECommerce HUIT

> Giao diện người dùng cho hệ thống bán hàng điện tử HUIT

---

## 🎨 Tech Stack

- **Framework:** React 18 (hoặc Vue 3 / Next.js tùy chọn)
- **Language:** TypeScript
- **Styling:** Tailwind CSS / SCSS
- **State Management:** Redux Toolkit / Zustand
- **API Client:** Axios / Fetch
- **Forms:** React Hook Form + Zod validation
- **Routing:** React Router v6
- **UI Components:** Headless UI / Radix UI / Material-UI
- **Icons:** Heroicons / Font Awesome
- **Images:** Next.js Image (nếu dùng Next) hoặc lazy loading

---

## 📁 Folder Structure

```
FRONTEND/
├── public/
│   ├── index.html
│   ├── favicon.ico
│   └── manifest.json (PWA)
├── src/
│   ├── api/                    # API client & service layer
│   │   ├── client.ts           # Axios instance với interceptors
│   │   ├── authApi.ts
│   │   ├── productApi.ts
│   │   ├── orderApi.ts
│   │   └── adminApi.ts
│   ├── components/             # Reusable UI components
│   │   ├── common/
│   │   │   ├── Button.tsx
│   │   │   ├── Modal.tsx
│   │   │   ├── LoadingSpinner.tsx
│   │   │   ├── Toast.tsx
│   │   │   └── Pagination.tsx
│   │   ├── layout/
│   │   │   ├── Header.tsx
│   │   │   ├── Footer.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── Layout.tsx
│   │   ├── product/
│   │   │   ├── ProductCard.tsx
│   │   │   ├── ProductGrid.tsx
│   │   │   └── ProductFilter.tsx
│   │   ├── cart/
│   │   │   ├── CartItem.tsx
│   │   │   └── CartSummary.tsx
│   │   ├── orders/
│   │   │   ├── OrderCard.tsx
│   │   │   └── OrderTimeline.tsx
│   │   └── admin/
│   │       ├── DataTable.tsx
│   │       └── StatCard.tsx
│   ├── pages/                  # Page components
│   │   ├── HomePage.tsx
│   │   ├── ProductListPage.tsx
│   │   ├── ProductDetailPage.tsx
│   │   ├── CartPage.tsx
│   │   ├── CheckoutPage.tsx
│   │   ├── OrderSuccessPage.tsx
│   │   ├── MyOrdersPage.tsx
│   │   ├── OrderDetailPage.tsx
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── ProfilePage.tsx
│   │   └── admin/
│   │       ├── AdminDashboard.tsx
│   │       ├── AdminProducts.tsx
│   │       ├── AdminOrders.tsx
│   │       ├── AdminInventory.tsx
│   │       └── AdminReports.tsx
│   ├── store/                  # State management (Redux/Zustand)
│   │   ├── slices/
│   │   │   ├── authSlice.ts
│   │   │   ├── cartSlice.ts
│   │   │   ├── productSlice.ts
│   │   │   └── orderSlice.ts
│   │   ├── store.ts
│   │   └── hooks.ts
│   ├── hooks/                  # Custom React hooks
│   │   ├── useAuth.ts
│   │   ├── useCart.ts
│   │   ├── useProducts.ts
│   │   └── usePagination.ts
│   ├── utils/                  # Helper functions
│   │   ├── format.ts           # format currency, date
│   │   ├── validation.ts
│   │   └── constants.ts
│   ├── types/                  # TypeScript type definitions
│   │   ├── product.ts
│   │   ├── order.ts
│   │   ├── user.ts
│   │   └── api.ts
│   ├── styles/                 # Global styles, Tailwind imports
│   │   └── globals.css
│   ├── App.tsx
│   └── main.tsx                # Entry point
├── package.json
├── vite.config.ts / next.config.js / craco.config.js
├── tailwind.config.js
├── tsconfig.json
├── .env.example
├── .env.local (gitignored)
└── README.md
```

---

## 🚀 Quick Start

### Prerequisites

- Node.js 18+
- npm or yarn

### 1. Install Dependencies

```bash
cd FRONTEND
npm install
```

### 2. Configure Environment

Copy `.env.example` to `.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_GOOGLE_CLIENT_ID=your_client_id (optional)
```

### 3. Run Development Server

```bash
npm run dev
```

App sẽ chạy tại `http://localhost:3000` (với Vite) hoặc `http://localhost:5173`.

### 4. Build for Production

```bash
npm run build
```

Output vào `dist/` folder (chứa static files). Có thể serve bằng bất kỳ web server nào (Nginx, Apache, Vercel, Netlify,...).

---

## 🔌 API Integration

### API Service (`src/api/client.ts`)

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
});

// Request interceptor: thêm JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('access_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor: handle 401, refresh token
api.interceptors.response.use(
  (response) => response.data,
  async (error) => {
    if (error.response?.status === 401) {
      // Gọi refresh token endpoint
      const refreshToken = localStorage.getItem('refresh_token');
      const newToken = await refreshAccessToken(refreshToken);
      localStorage.setItem('access_token', newToken);
      error.config.headers.Authorization = `Bearer ${newToken}`;
      return api(error.config);
    }
    return Promise.reject(error);
  }
);

export default api;
```

### Example Service (`src/api/productApi.ts`)

```typescript
import api from './client';

export const productApi = {
  getProducts(params: GetProductsParams) {
    return api.get('/api/products', { params });
  },
  getProductById(id: number) {
    return api.get(`/api/products/${id}`);
  },
};
```

---

## 🎯 Key Pages & Components

### Public Pages
- **HomePage:** Banner, featured products, categories
- **ProductListPage:** Grid sản phẩm với filter (category, brand, price, sort)
- **ProductDetailPage:** Chi tiết SP, chọn variant, giỏ hàng
- **CartPage:** Xem giỏ hàng, cập nhật số lượng, apply voucher
- **CheckoutPage:** Nhập địa chỉ, chọn payment, xác nhận
- **OrderSuccessPage:** Thông báo đặt hàng thành công, tracking link
- **Login/Register:** Authentication forms

### Customer Pages (authenticated)
- **MyOrdersPage:** Lịch sử đơn hàng
- **OrderDetailPage:** Chi tiết đơn, tracking, yêu cầu return
- **ProfilePage:** Thông tin cá nhân, địa chỉ

### Admin Pages (role ADMIN/STAFF)
- **AdminDashboard:** Tổng quan doanh thu, đơn hàng mới
- **AdminProducts:** CRUD sản phẩm, variants, images
- **AdminOrders:** Danh sách đơn, cập nhật trạng thái
- **AdminInventory:** Xem tồn kho, nhập/xuất/chuyển kho
- **AdminReports:** Báo cáo doanh thu, best sellers

---

## 💅 Styling với Tailwind CSS

Tailwind config:

```js
// tailwind.config.js
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#eff6ff',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
        },
        // ...
      },
    },
  },
  plugins: [],
}
```

Example component:

```tsx
const ProductCard = ({ product }: { product: Product }) => (
  <div className="border rounded-lg overflow-hidden shadow-sm hover:shadow-md transition">
    <img src={product.thumbnail_url} alt={product.name} className="w-full h-48 object-cover" />
    <div className="p-4">
      <h3 className="font-semibold text-lg">{product.name}</h3>
      <p className="text-gray-500">{product.variant_name}</p>
      <div className="mt-2 flex items-center justify-between">
        <span className="text-xl font-bold text-red-600">
          {formatCurrency(product.price)}
        </span>
        <button className="bg-primary-600 text-white px-4 py-2 rounded hover:bg-primary-700">
          Thêm vào giỏ
        </button>
      </div>
    </div>
  </div>
);
```

---

## 🔄 State Management (Redux Toolkit example)

### Store Setup

```typescript
// src/store/store.ts
import { configureStore } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice';
import cartReducer from './slices/cartSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    cart: cartReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
```

### Cart Slice

```typescript
// src/store/slices/cartSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { cartApi } from '../../api/cartApi';

interface CartItem {
  id: number;
  variant_id: number;
  quantity: number;
  variant: {
    sku: string;
    variant_name: string;
    price: number;
    thumbnail_url: string;
  };
}

interface CartState {
  items: CartItem[];
  subtotal: number;
  discount: number;
  total: number;
  appliedVoucher: Voucher | null;
  loading: boolean;
}

const initialState: CartState = {
  items: [],
  subtotal: 0,
  discount: 0,
  total: 0,
  appliedVoucher: null,
  loading: false,
};

export const addToCart = createAsyncThunk(
  'cart/add',
  async ({ variant_id, quantity }: { variant_id: number; quantity: number }) => {
    return await cartApi.addItem(variant_id, quantity);
  }
);

const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {
    removeItem: (state, action: PayloadAction<number>) => {
      state.items = state.items.filter(item => item.id !== action.payload);
    },
    updateQuantity: (state, action: PayloadAction<{ id: number; quantity: number }>) => {
      const item = state.items.find(i => i.id === action.payload.id);
      if (item) item.quantity = action.payload.quantity;
    },
    clearCart: (state) => {
      state.items = [];
      state.appliedVoucher = null;
    },
    setVoucher: (state, action: PayloadAction<Voucher | null>) => {
      state.appliedVoucher = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(addToCart.fulfilled, (state, action) => {
        const existing = state.items.find(i => i.variant_id === action.payload.variant_id);
        if (existing) {
          existing.quantity += action.payload.quantity;
        } else {
          state.items.push(action.payload);
        }
        // recalc totals
        state.subtotal = state.items.reduce((sum, i) => sum + i.quantity * i.variant.price, 0);
      });
  },
});

export const { removeItem, updateQuantity, clearCart, setVoucher } = cartSlice.actions;
export default cartSlice.reducer;
```

---

## 🧪 Testing

### Unit Tests (Jest + React Testing Library)

```bash
npm test
```

Example test:

```tsx
// ProductCard.test.tsx
import { render, screen, fireEvent } from '@testing-library/react';
import ProductCard from './ProductCard';

test('renders product name and price', () => {
  const product = {
    id: 1,
    name: 'iPhone 15 Pro Max',
    variant_name: '256GB - Titan',
    price: 28990000,
    thumbnail_url: 'https://example.com/iphone.jpg',
  };
  render(<ProductCard product={product} />);
  expect(screen.getByText('iPhone 15 Pro Max')).toBeInTheDocument();
  expect(screen.getByText('256GB - Titan')).toBeInTheDocument();
  expect(screen.getByText('28.990.000 ₫')).toBeInTheDocument();
});
```

### E2E Tests with Cypress

```bash
npm run cypress:open
```

---

## 📱 Responsive Design

Tailwind breakpoints:
- `sm:` (640px)
- `md:` (768px)
- `lg:` (1024px)
- `xl:` (1280px)
- `2xl:` (1536px)

Ensure all pages work on mobile, tablet, desktop.

---

## 🌐 Internationalization (i18n)

Nếu cần hỗ trợ nhiều ngôn ngữ:

```bash
npm install react-i18next i18next
```

Tạo `src/locales/vi.json`, `en.json`.

---

## 📦 Deployment

### Build

```bash
npm run build
```

### Serve (static)

```bash
npm install -g serve
serve -s dist
```

### Vercel / Netlify

- Push lên GitHub
- Import repo vào Vercel/Netlify
- Set environment variables
- Deploy

---

## 🔐 Security Best Practices

- Never commit `.env.local`
- Sanitize user input khi hiển thị (XSS)
- Validate trên client nhưng critical phải validate trên server
- Không lưu JWT trong localStorage nếu cần bảo mật cao → dùng httpOnly cookies
- Implement CSP headers
- Rate limiting trên API (đã có ở backend)

---

## 🐛 Debugging

- Use React DevTools (browser extension)
- Network tab để xem API calls
- Console log với `debug` library
- Redux DevTools (nếu dùng Redux)

---

## 📚 Resources

- [React Docs](https://react.dev/)
- [Tailwind CSS](https://tailwindcss.com/)
- [Redux Toolkit](https://redux-toolkit.js.org/)
- [Vite](https://vitejs.dev/)

---

## 🙋 Need Help?

Tạo issue trong GitHub repository hoặc tham khảo backend API docs.

Happy coding! 🚀
