# Changelog

Tất cả thay đổi đáng chú ý của dự án AutoX.Gara sẽ được ghi lại trong file này.

Định dạng dựa trên [Keep a Changelog](https://keepachangelog.com/vi/1.0.0/), phiên bản dự án tuân theo [Semantic Versioning](https://semver.org/lang/vi/).

---

## [Unreleased]

### Planned

- Cải thiện tài liệu và hướng dẫn triển khai.
- Tùy chọn TLS cho kết nối client–server.
- Mở rộng RBAC và audit log.

---

## [1.0.0] - 2026-03-xx

### Added

- Ứng dụng client .NET MAUI 10 (Windows): đăng nhập, quản lý khách hàng, nhân viên, xe, phụ tùng, dịch vụ, nhà cung cấp, đơn sửa chữa, hóa đơn, giao dịch.
- Server .NET 10 console: TCP listener (Nalix.Network), xử lý message đăng nhập và CRUD các module.
- Kiến trúc phân lớp: Domain, Application, Infrastructure, Shared, Backend, Frontend.
- Database: SQLite (mặc định), PostgreSQL (tùy chọn); EF Core 10 Code First, migrations.
- Giao tiếp client–server qua Nalix: request/response, packet dispatch.
- Logging: Nalix.Logging phía server; cấu hình qua options.

### Documentation

- README.md: mô tả dự án, tính năng, build & chạy, công nghệ.
- ARCHITECTURE.md: kiến trúc tổng quan, lớp, luồng giao tiếp, bảo mật.
- CONTRIBUTING.md, CODE_OF_CONDUCT.md, SECURITY.md, LICENSE (Apache-2.0).
- docs/GETTING_STARTED.md: hướng dẫn bắt đầu nhanh.
- docs/media: preview giao diện (login, customer, employees, parts, services, suppliers).

---

[Unreleased]: https://github.com/ppn-systems/AutoX.Gara/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/ppn-systems/AutoX.Gara/releases/tag/v1.0.0
