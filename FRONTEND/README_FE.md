# Dự án Frontend - ECommerce HUIT

> Giao diện người dùng cho hệ thống bán hàng điện tử HUIT

## 🚀 Quick Start

### Prerequisites
- Node.js 18+
- npm or yarn

### Installation
```bash
cd FRONTEND
npm install
```

### Development
```bash
npm run dev
```
App sẽ chạy tại `http://localhost:5173`.

### Build
```bash
npm run build
```
Build output vào `dist/`.

### Test
```bash
npm test
```

### Environment
Copy `.env.example` to `.env.local`:
```env
VITE_API_BASE_URL=http://localhost:5000
```

---

## 📦 Tech Stack
- **React 18** + TypeScript
- **Vite** (build tool)
- **Tailwind CSS** (styling)
- **React Router v6** (routing)
- **Zustand** (state management)
- **React Query** (data fetching)
- **React Hook Form** + **Zod** (forms)
- **Axios** (HTTP client)
- **Lucide React** (icons)

---

## 📁 Structure
```
src/
├── api/           # API clients
├── components/    # Reusable UI components
├── pages/         # Page components
├── store/         # Zustand stores
├── hooks/         # Custom hooks (todo)
├── types/         # TypeScript definitions
├── utils/         # Helpers
├── styles/        # Global CSS
├── App.tsx
└── main.tsx
```

---

## 🔌 API Integration
- All API calls use `axios` with interceptor for JWT
- Base URL from `VITE_API_BASE_URL`
- 401 handling: auto refresh token or redirect to login
- Errors shown via toast notifications

---

## 🏗️ Architecture
- **Routing**: React Router v6 with nested layout
- **State**: Zustand with persist (localStorage)
- **Data fetching**: React Query for caching, background refetch
- **Styling**: Tailwind CSS with custom color theme (`primary` palette)
- **Forms**: React Hook Form + Zod validation (todo)
- **Components**: Headless UI approach (no MUI), building from scratch

---

## ✅ Pages Implemented
- HomePage: Hero + featured products
- ProductListPage: Grid with filters (category, price, search, pagination)
- ProductDetailPage: Product details, variant selection, quantity, add to cart
- CartPage: View/update cart, apply voucher, cart summary
- CheckoutPage: Shipping address, payment method, submit order
- OrderSuccessPage: Confirmation with order code
- LoginPage, RegisterPage: Auth forms

---

## 🧪 Testing
Vitest + Testing Library setup ready. Test files co-located with components.

Run:
```bash
npm test
```

---

## 🚢 Deployment
Build with `npm run build`, deploy static files to:
- Vercel
- Netlify
- Nginx (any web server)

---

## 💅 Styling Notes
- Primary color: `#3b82f6` (Tailwind blue-500)
- Uses `clsx` for conditional classes
- Responsive with Tailwind breakpoints (sm, md, lg, xl)
- Rounded-lg for cards and buttons
- Shadow and border for card surfaces

---

**Happy coding!** 🎉
