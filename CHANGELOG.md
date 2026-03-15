# 📋 Changelog

Tất cả thay đổi đáng chú ý của **AutoX.Gara** được ghi lại trong file này.

- Định dạng: [Keep a Changelog](https://keepachangelog.com/vi/1.0.0/)
- Phiên bản: [Semantic Versioning](https://semver.org/lang/vi/)

---

## [Unreleased]

### 📌 Planned

- Cải thiện tài liệu và hướng dẫn triển khai
- Tùy chọn TLS cho kết nối client–server
- Mở rộng RBAC và audit log

---

## [1.0.0] - 2026-03-xx

### ✨ Added

| Loại | Nội dung |
|------|----------|
| 🖥️ **Client** | .NET MAUI 10 (Windows): đăng nhập, CRUD khách hàng, nhân viên, xe, phụ tùng, dịch vụ, nhà cung cấp, đơn sửa chữa, hóa đơn, giao dịch |
| ⚙️ **Server** | .NET 10 console: TCP listener (Nalix.Network), xử lý message đăng nhập và CRUD các module |
| 🏗️ **Kiến trúc** | Domain, Application, Infrastructure, Shared, Backend, Frontend |
| 🗄️ **Database** | SQLite (mặc định), PostgreSQL (tùy chọn); EF Core 10 Code First, migrations |
| 📡 **Giao tiếp** | Request/response qua Nalix, packet dispatch |
| 📝 **Logging** | Nalix.Logging phía server |

### 📚 Documentation

- README, ARCHITECTURE, CONTRIBUTING, CODE_OF_CONDUCT, SECURITY, LICENSE (Apache-2.0)
- docs/GETTING_STARTED, CONFIGURATION, PROTOCOL, DEPLOYMENT, TROUBLESHOOTING, GLOSSARY
- docs/MODULES, DATABASE, FAQ, OPERATIONS, ROADMAP, RELEASE, DEVELOPMENT, COMPATIBILITY, THIRD_PARTY
- docs/media: preview giao diện (login, customer, employees, parts, services, suppliers)

---

[Unreleased]: https://github.com/ppn-systems/AutoX.Gara/compare/v1.0.0...HEAD  
[1.0.0]: https://github.com/ppn-systems/AutoX.Gara/releases/tag/v1.0.0
