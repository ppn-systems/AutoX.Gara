# 🚗 Nalix Garage Management System

Hệ thống **Desktop Client–Server** dùng cho quản lý garage / kho / sửa chữa, xây dựng trên nền **.NET + Avalonia + Nalix.Network**.

Mục tiêu chính:

- Hiệu năng cao (high performance)
- Bảo mật (security)
- Dễ bảo trì & mở rộng (maintainability & extensibility)
- Phù hợp cho team nhỏ → có thể scale dần theo thời gian

---

## 🧩 Tổng quan kiến trúc

- **Client (Máy khách)**: Avalonia UI (target .NET 8)
- **Server (Máy chủ)**: .NET 10 (Console / Worker / Minimal Host)
- **Giao tiếp (Communication)**: TCP qua **Nalix.Network**
- **Cơ sở dữ liệu (Database – DB)**: SQL Server Express
- **ORM**: Entity Framework Core (EF Core)
- **Xác thực & phân quyền (Auth)**: Server-side RBAC (Role-Based Access Control)
- **Đa ngôn ngữ (Localization)**: Tiếng Việt / Tiếng Anh
- **Realtime**: Server push events (đẩy sự kiện thời gian thực từ Server xuống Client)

---

## 📦 Các thành phần chính

| Layer       | Công nghệ / Mô tả                                       |
|------------ |---------------------------------------------------------|
| UI          | Avalonia (.NET 8) – Desktop Client                      |
| Business    | .NET 10 – Domain & Application logic                    |
| Network     | Nalix.Network – TCP, message framing, serialization     |
| Database    | SQL Server Express                                      |
| ORM         | EF Core – Code First + Migrations                       |
| Logging     | Nalix.Logging (wrapper, structured logging)             |
| Auth        | RBAC – kiểm soát truy cập theo vai trò trên Server      |
| Localization| VN / EN – resource files / resx / JSON                  |
| Realtime    | Server push events – pub/sub, notifications             |

---

**Lưu ý (Note)**:

> - `AutoX.Gara.Domain` nên thuần domain (DDD) – tránh phụ thuộc EF Core trực tiếp.  
> - `AutoX.Gara.Data` implement các interface từ `AutoX.Gara.Domain` (Repository, Unit of Work).  
> - `AutoX.Gara.Server` là Application Layer: xử lý use case, RBAC, orchestration.

---
