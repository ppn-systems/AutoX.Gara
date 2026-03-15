# 🏗️ Kiến trúc AutoX.Gara

> Tài liệu mô tả kiến trúc thực tế: **client–server**, .NET 10, MAUI, Nalix.Network, EF Core.

---

## 📋 1. Tổng quan

| Thành phần | Công nghệ / Mô tả |
|------------|-------------------|
| 🖥️ **Client (UI)** | .NET MAUI 10 — ứng dụng desktop Windows |
| ⚙️ **Server** | .NET 10 console — TCP listener, xử lý message Nalix |
| 📡 **Giao tiếp** | TCP qua **Nalix.Network** (message framing, request/response) |
| 🗄️ **Database** | **SQLite** (mặc định) hoặc **PostgreSQL** (cấu hình) |
| 🔄 **ORM** | Entity Framework Core 10 (Code First, migrations) |
| 🔐 **Auth** | Server-side (RBAC mở rộng); client gửi token |
| 📝 **Logging** | Nalix.Logging (structured logging) |

---

## 🧱 2. Kiến trúc phân lớp (Layered)

```text
┌───────────────────────────────────────────────────────────┐
│  🖥️ Presentation (UI)                                     │
│  AutoX.Gara.Frontend — MAUI, MVVM (CommunityToolkit)      │
└──────────────────────────┬────────────────────────────────┘
                           │ TCP / Nalix (Request–Response)
┌──────────────────────────▼────────────────────────────────┐
│  ⚙️ Application Layer                                      │
│  AutoX.Gara.Application — Use case, message handlers      │
│  (CustomerOps, EmployeeOps, SparePartOps, VehicleOps, …)   │
└──────────────────────────┬────────────────────────────────┘
                           │
┌──────────────────────────▼────────────────────────────────┐
│  🧬 Domain Layer                                          │
│  AutoX.Gara.Domain — Entities, value objects (DDD)        │
└──────────────────────────┬────────────────────────────────┘
                           │
┌──────────────────────────▼────────────────────────────────┐
│  🔌 Infrastructure Layer                                   │
│  DbContext, Repositories, AutoXListener, AutoXProtocol     │
└──────────────────────────┬────────────────────────────────┘
                           │
┌──────────────────────────▼────────────────────────────────┐
│  🗄️ Database — SQLite / PostgreSQL (EF Core)               │
└───────────────────────────────────────────────────────────┘
```

> **Shared** (`AutoX.Gara.Shared`): DTO, protocol (request/response), cấu hình dùng chung Client & Server.

---

## 📦 3. Trách nhiệm từng project

| Project | Trách nhiệm |
|---------|-------------|
| **AutoX.Gara.Domain** | Entity, business rule thuần; không reference EF/Infrastructure |
| **AutoX.Gara.Application** | Use case, xử lý message Nalix; gọi repository/domain |
| **AutoX.Gara.Infrastructure** | DbContext, Repository, Nalix listener/protocol, cấu hình DB |
| **AutoX.Gara.Shared** | DTO, request/response, packet format (Frontend + Backend) |
| **AutoX.Gara.Backend** | Điểm vào server: cấu hình Nalix, đăng ký handler, TCP listener |
| **AutoX.Gara.Frontend** | MAUI: ViewModels, Views, Nalix SDK gửi/nhận message |

---

## 🔄 4. Luồng giao tiếp (Client ↔ Server)

| Bước | Mô tả |
|------|--------|
| **1. Kết nối TCP** | Client (Nalix.SDK) → server (AutoXListener + AutoXProtocol); đăng ký ConnectionHub |
| **2. Đăng nhập** | Client gửi LoginPacket → server kiểm tra hash/salt → trả token → client gắn token mọi request sau |
| **3. Command/Query** | Client gửi request (CustomerQueryRequest, PartQueryRequest…) + token → server xác thực, RBAC, Ops → Repository/Domain → trả DTO |
| **4. Push event** | (Tùy chọn) Server đẩy sự kiện qua Nalix tới client subscribe |

---

## 🗃️ 5. Mô hình dữ liệu (core entities)

| Nhóm | Entity |
|------|--------|
| 🔐 **Identity** | Account, Employee (Role/Permission khi có RBAC) |
| 👥 **Customer** | Customer, Vehicle |
| 📦 **Inventory** | Part, Supplier |
| 📋 **Repair** | RepairOrder, RepairTask, RepairOrderItem |
| 🧾 **Billing** | Invoice, Transaction |

> **Quan hệ**: Customer → Vehicle; Invoice ↔ RepairOrder; RepairOrder → RepairTask (dịch vụ) + RepairOrderItem (phụ tùng); Transaction → Invoice.

➡️ Chi tiết: `AutoX.Gara.Domain`, [docs/MODULES.md](docs/MODULES.md), [docs/DATABASE.md](docs/DATABASE.md)

---

## 🗄️ 6. Database

| Mục | Ghi chú |
|-----|--------|
| **Mặc định** | SQLite (`AutoX.db`), `DatabaseOptions` (Infrastructure) |
| **Tùy chọn** | PostgreSQL: `DatabaseType = "PostgreSQL"` + `ConnectionString` |
| **Migrations** | EF Core Code First; `AutoXDbContextFactory` cho `dotnet ef` |

> Server là **single source of truth**; client không truy cập DB trực tiếp.

---

## 🛡️ 7. Bảo mật (khuyến nghị)

| Khía cạnh | Khuyến nghị |
|------------|-------------|
| 📡 **Giao tiếp** | TLS (SslStream/Nalix); không gửi mật khẩu plain text |
| 🔑 **Mật khẩu** | Salted hash (PBKDF2/Argon2) phía server |
| 🧩 **Phân quyền** | RBAC trên server cho mọi command |
| ✅ **Đầu vào** | Validate độ dài, kiểu, range trên server |
| 🧯 **Truy vấn** | EF Core / tham số hóa; tránh SQL injection |
| 📝 **Log** | Không log mật khẩu/token; log rotation |
| 🚫 **Brute-force** | Rate limiting, lockout đăng nhập sai |

---

## 🔮 8. Mở rộng sau này

- **Offline / sync**: Client cache (SQLite/LiteDB) + sync; server authoritative.
- **Auto-update**: HTTP/HTTPS manifest + tải bản cập nhật (Squirrel, MSIX).
- **Localization**: Resource (.resx/JSON), đa ngôn ngữ VN/EN, chuyển runtime.
- **Audit**: AuditLog (login, CRUD) phía server; rotation theo chính sách.

---

## 📚 Tài liệu liên quan

| Tài liệu | Nội dung |
|----------|----------|
| [docs/MODULES.md](docs/MODULES.md) · [docs/DATABASE.md](docs/DATABASE.md) | Chi tiết module, entity, quan hệ |
| [README.md](README.md) | Build, chạy, tính năng |
| [docs/GETTING_STARTED.md](docs/GETTING_STARTED.md) | Bắt đầu nhanh |
