# Contributing to ECommerce HUIT

Cảm ơn bạn đ quan tâm đóng góp cho dự án! Hướng dẫn này sẽ giúp bạn bắt đầu.

---

## How to Contribute

### 1. Fork the Repository
Click "Fork" button trên GitHub để tạo bản sao của repo về tài khoản của bạn.

### 2. Clone Your Fork
```bash
git clone https://github.com/YOUR-USERNAME/ecommerce-huit.git
cd ecommerce-huit
```

### 3. Add Upstream Remote
```bash
git remote add upstream https://github.com/Johnyyd/ecommerce-huit.git
```

### 4. Create a Branch
```bash
git checkout -b feature/AmazingFeature
# hoặc
git checkout -b fix/issue-123
```

### 5. Make Changes
- Tuân thủ coding conventions (dùng .editorconfig nếu có)
- Viết unit tests cho code mới
- Cập nhật documentation nếu cần

### 6. Commit Changes
```bash
git add .
git commit -m "feat: add new inventory transfer API"
# hoặc
git commit -m "fix: correct stock calculation in sp_CreateOrder"
```

**Commit Message Convention (Conventional Commits):**
- `feat:` tính năng mới
- `fix:` sửa lỗi
- `docs:` chỉnh sửa tài liệu
- `style:` định dạng, thiếu dấu chấm câu (không ảnh hưởng code)
- `refactor:` refactor code
- `test:` thêm test
- `chore:` công việc build, tooling

### 7. Pull Latest from Upstream
```bash
git pull upstream main --rebase
```

Xử lý merge conflicts nếu có.

### 8. Push to Your Fork
```bash
git push origin feature/AmazingFeature
```

### 9. Open a Pull Request
- Vào repo gốc (Johnyyd/ecommerce-huit)
- Click "Compare & pull request"
- Chọn branch của bạn so sánh với `main`
- Điền mô tả chi tiết: **What?** **Why?** **How?**
- Liên kết issue nếu có (ví dụ: "Closes #12")

### 10. Code Review
- Maintainer sẽ review code
- Hãy phản hồi review comments
- Cập nhật PR nếu cần sửa

### 11. Merge
Sau khi approve, PR sẽ được merge vào `main`.

---

## Development Setup

### Database
1. Install SQL Server 2019+ hoặc dùng Docker:
   ```bash
   docker-compose up -d sqlserver
   ```
2. Chạy init.sql:
   ```bash
   sqlcmd -S localhost -U SA -P 'YourStrong@Passw0rd' -i DATABASE/init.sql
   ```
3. Chạy seed.sql nếu muốn có dữ liệu mẫu.

### Backend (ASP.NET Core)
```bash
cd BACKEND/src
dotnet restore
cp appsettings.example.json appsettings.json
# Sửa connection strings, JWT secret
dotnet run
```

### Frontend (React/Vue)
```bash
cd FRONTEND/src
npm install
npm run dev
```

---

## Coding Standards

### SQL
- Dùng NVARCHAR cho text tiếng Việt
- Thêm comment cho stored procedure, complex query
- Đặt tên bảng, column: snake_case (hoặc PascalCase cho đối tượng)
- Index cho foreign keys và query thường xuyên
- CHECK constraints cho enum-like values

### C# (backend)
- Follow C# coding conventions (PascalCase cho public, camelCase cho private)
- Use async/await for I/O operations
- Validate input với FluentValidation
- Log với Serilog (structured logging)
- Unit test với xUnit/NUnit

### JavaScript/TypeScript (frontend)
- TypeScript recommended
- Functional components với React Hooks
- Use environment variables cho config

---

## Testing

### Database
- Stored procedure: test với various inputs
- Xử lý transaction, error cases

### Backend
- Unit tests: test logic riêng
- Integration tests: test API endpoints với testdatabase
- Sử dụng WebApplicationFactory cho ASP.NET Core

### Frontend
- Component tests (Jest, React Testing Library)
- E2E tests (Cypress, Playwright)

---

## Issue Reporting

When reporting bugs, please include:

1. **Mô tả** rõ ràng
2. **Bước để reproduce** (step-by-step)
3. **Kết quả mong đợi** vs **thực tế**
4. **Môi trường** (OS, DB version, .NET version, browser,...)
5. Screenshots/logs nếu có

---

## Feature Requests

- Mô tả feature rõ ràng
- Giải thích **use case** và **benefit**
- Nếu có thể, vẽ wireframe/UI mockup
- Thảo luận trước khi code (tạo issue để discussion)

---

## Pull Request Checklist

Before submitting a PR, ensure:

- [ ] Code compiles without errors
- [ ] All tests pass (unit + integration)
- [ ] No merge conflicts with upstream main
- [ ] Added/updated documentation (README, API docs, code comments)
- [ ] No sensitive data (passwords, keys) committed
- [ ] Follows project''s coding style
- [ ] Commit messages follow Conventional Commits

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License (see LICENSE file).

---

## Questions?

Gửi message trực tiếp qua GitHub Issues hoặc Discussions.

Thank you for contributing! 👏
