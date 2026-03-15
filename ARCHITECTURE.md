# Kiến trúc AutoX.Gara

Tài liệu mô tả kiến trúc thực tế của hệ thống **AutoX.Gara** (client–server, .NET 10, MAUI, Nalix.Network, EF Core).

---

## 1. Tổng quan

- **Client (UI)**: .NET MAUI 10 — ứng dụng desktop Windows.
- **Server**: .NET 10 console — host TCP listener, xử lý message Nalix.
- **Giao tiếp**: TCP qua **Nalix.Network** (message framing, request/response, có thể mở rộng push event).
- **Database**: **SQLite** (mặc định) hoặc **PostgreSQL** (cấu hình); chỉ chạy phía server.
- **ORM**: Entity Framework Core 10 (Code First, migrations).
- **Xác thực / phân quyền**: Server-side (RBAC có thể mở rộng); client gửi đăng nhập/token, server kiểm tra.
- **Logging**: Nalix.Logging (structured logging).

---

## 2. Kiến trúc phân lớp (Layered)

```text
┌───────────────────────────────────────────────────────────┐
│  Presentation (UI)                                        │
│  AutoX.Gara.Frontend — MAUI, MVVM (CommunityToolkit)      │
└──────────────────────────┬────────────────────────────────┘
                           │ TCP / Nalix (Request–Response)
┌──────────────────────────▼────────────────────────────────┐
│  Application Layer                                        │
│  AutoX.Gara.Application — Use case, message handlers      │
│  (CustomerOps, EmployeeOps, SparePartOps, VehicleOps, …)  │
└──────────────────────────┬────────────────────────────────┘
                           │
┌──────────────────────────▼────────────────────────────────┐
│  Domain Layer                                             │
│  AutoX.Gara.Domain — Entities, value objects (DDD)        │
│  (không phụ thuộc EF Core / Infrastructure)               │
└──────────────────────────┬────────────────────────────────┘
                           │
┌──────────────────────────▼────────────────────────────────┐
│  Infrastructure Layer                                     │
│  AutoX.Gara.Infrastructure — DbContext, Repositories,     │
│  Nalix listener (AutoXListener), protocol (AutoXProtocol) │
└──────────────────────────┬────────────────────────────────┘
                           │
┌──────────────────────────▼────────────────────────────────┐
│  Database — SQLite / PostgreSQL (EF Core)                 │
└───────────────────────────────────────────────────────────┘
```

- **Shared** (`AutoX.Gara.Shared`): DTO, protocol (request/response), cấu hình dùng chung giữa Client và Server.

---

## 3. Cấu trúc solution và trách nhiệm từng project

| Project | Trách nhiệm |
|---------|-------------|
| **AutoX.Gara.Domain** | Entity, business rule thuần domain; không reference EF hay Infrastructure. |
| **AutoX.Gara.Application** | Use case: xử lý command/query tương ứng message Nalix; gọi repository/domain. |
| **AutoX.Gara.Infrastructure** | EF Core `DbContext`, repository implementation, Nalix `IListener`, `AutoXProtocol`, cấu hình DB. |
| **AutoX.Gara.Shared** | Contract: DTO, request/response, packet format; dùng bởi Frontend và Backend. |
| **AutoX.Gara.Backend** | Điểm vào server: cấu hình Nalix, đăng ký handler, chạy TCP listener. |
| **AutoX.Gara.Frontend** | Ứng dụng MAUI: ViewModels, Views, gọi Nalix SDK để gửi/nhận message. |

---

## 4. Luồng giao tiếp (Client ↔ Server)

1. **Kết nối TCP**  
   Client (Nalix.SDK) kết nối tới server (AutoXListener + AutoXProtocol). Protocol đăng ký connection với `ConnectionHub`.

2. **Đăng nhập**  
   - Client gửi message đăng nhập (ví dụ `LoginPacket` / request chứa username/password).  
   - Server: kiểm tra (hash/salt), tạo session/token, trả về response.  
   - Client lưu token và gắn vào các request sau.

3. **Thực thi lệnh (Command/Query)**  
   - Client gửi request (ví dụ `CustomerQueryRequest`, `PartQueryRequest`, …) kèm token.  
   - Server: xác thực token, (RBAC nếu có), gọi Application layer (Ops), Application gọi Repository/Domain, trả về DTO (response).  
   - Message được dispatch qua `IPacketDispatch` → handler tương ứng trong Application.

4. **Push event (tùy chọn)**  
   Có thể mở rộng: server đẩy sự kiện (thay đổi tồn kho, cập nhật đơn sửa, …) qua kênh Nalix tới các client đã subscribe.

---

## 5. Mô hình dữ liệu (core entities)

- **Identity**: Account, Employee (và Role/Permission khi có RBAC).
- **Customer**: Customer, Vehicle (Customer ↔ Vehicle).
- **Inventory**: Part, Supplier (và liên kết nhà cung cấp – phụ tùng).
- **Repair**: RepairOrder, RepairTask, RepairOrderItem (đơn sửa chữa, hạng mục công việc, phụ tùng sử dụng).
- **Billing**: Invoice, Transaction (hóa đơn, giao dịch thanh toán).

Quan hệ chính: **Customer** → **Vehicle**; **Invoice** gắn **RepairOrder**; **RepairOrder** có **RepairTask** (dịch vụ) và **RepairOrderItem** (phụ tùng); **Transaction** gắn với thanh toán.

Chi tiết entity và relationship có thể xem trong `AutoX.Gara.Domain` và `AutoX.Gara.Infrastructure` (DbContext, migrations).

---

## 6. Database

- **Mặc định**: SQLite (file, ví dụ `AutoX.db`), connection string trong `DatabaseOptions` (Infrastructure).
- **Tùy chọn**: PostgreSQL — cấu hình `DatabaseType = "PostgreSQL"` và `ConnectionString` (Backend/Infrastructure).
- **Migrations**: EF Core Code First; design-time factory `AutoXDbContextFactory` dùng cho `dotnet ef`.

Server là **single source of truth**; client không truy cập DB trực tiếp.

---

## 7. Bảo mật (khuyến nghị)

- **Giao tiếp**: Dùng TLS (SslStream hoặc kênh mã hóa Nalix) cho kết nối từ xa; không gửi mật khẩu plain text.
- **Mật khẩu**: Lưu salted hash (PBKDF2/Argon2) phía server.
- **Phân quyền**: RBAC thực thi trên server cho mọi command; không tin client.
- **Đầu vào**: Validate mọi dữ liệu từ client trên server (độ dài, kiểu, range).
- **Truy vấn**: Dùng EF Core / tham số hóa, tránh SQL injection.
- **Log**: Không ghi mật khẩu, token, thông tin nhạy cảm; có log rotation.
- **Rate limiting**: Giới hạn đăng nhập sai (lockout) để chống brute-force.

---

## 8. Mở rộng sau này

- **Offline / sync**: Thêm client cache (ví dụ SQLite/LiteDB), cơ chế sync với server (version, conflict resolution); server vẫn authoritative.
- **Auto-update**: Thường dùng HTTP/HTTPS để tải bản cập nhật (Squirrel, MSIX hoặc custom); có thể thiết kế `UpdateService` tách biệt kênh tải.
- **Localization**: Client dùng resource (.resx/JSON) và service đa ngôn ngữ (VN/EN); chuyển ngôn ngữ runtime nếu cần.
- **Audit**: Ghi AuditLog (login, CRUD quan trọng) phía server; lưu trữ và rotation theo chính sách.

---

## 9. Tài liệu bổ sung

- **docs/Architecture.md**: Mô tả chi tiết hơn từng module (Identity, Customer, Inventory, Repair, Billing), entity, relationship, có thể giữ làm tài liệu nội bộ.
- **README.md**: Hướng dẫn build, chạy, tính năng.
- **docs/GETTING_STARTED.md**: Bước bắt đầu nhanh (cấu hình, chạy server/client, migration).
