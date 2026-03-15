# Thuật ngữ (Glossary)

Bảng thuật ngữ dùng trong dự án **AutoX.Gara** (Tiếng Việt / English) và giải thích ngắn gọn.

---

## A–C

| Thuật ngữ (VN)     | English           | Giải thích |
|--------------------|-------------------|------------|
| Đơn sửa chữa       | Repair Order      | Phiếu sửa xe; gắn với khách hàng, xe, hóa đơn; chứa RepairTask và RepairOrderItem. |
| Ứng dụng / Client  | Client / Frontend | Ứng dụng MAUI (máy trạm) mà người dùng mở để làm việc. |
| Công việc sửa chữa | Repair Task      | Một hạng mục dịch vụ trong đơn sửa chữa (ví dụ thay dầu, kiểm tra phanh); liên kết ServiceItem. |
| CRUD               | CRUD              | Create, Read, Update, Delete — các thao tác cơ bản trên dữ liệu. |
| DTO                | DTO               | Data Transfer Object — đối tượng dùng để truyền dữ liệu qua mạng (packet payload). |

---

## D–K

| Thuật ngữ (VN)   | English        | Giải thích |
|------------------|----------------|------------|
| Gara             | Garage         | Cơ sở sửa chữa, bảo dưỡng ô tô. |
| Hạng mục đơn sửa | Repair Order Item | Dòng phụ tùng dùng trong một đơn sửa chữa (số lượng, đơn giá, Part). |
| Hóa đơn          | Invoice        | Hóa đơn thanh toán; gắn với khách hàng, có thể gắn RepairOrder. |
| Kho / Phụ tùng   | Part / Inventory | Linh kiện, phụ tùng trong kho (Part); quản lý tồn kho. |
| Kiến trúc phân lớp | Layered Architecture | Chia hệ thống thành các lớp: UI, Application, Domain, Infrastructure. |

---

## L–P

| Thuật ngữ (VN)   | English         | Giải thích |
|------------------|-----------------|------------|
| Middleware       | Middleware      | Tầng xử lý trên luồng message (kiểm tra quyền, rate limit, unwrap/wrap packet). |
| Nhân viên        | Employee        | Người làm việc tại gara; có thể gắn Account (đăng nhập). |
| OpCode / OpCommand | OpCode        | Mã lệnh trong packet (ví dụ LOGIN, CUSTOMER_GET) để server biết loại thao tác. |
| Ops              | Ops             | Handler xử lý một hoặc nhiều OpCode trong Application layer (ví dụ CustomerOps, EmployeeOps). |
| Packet           | Packet          | Đơn vị message gửi/nhận qua Nalix: có header (OpCode, length, flags) và payload (serialize). |
| Phân trang       | Pagination      | Trả dữ liệu theo trang (Page, PageSize) để tránh load quá nhiều bản ghi. |
| PostgreSQL       | PostgreSQL      | Hệ quản trị cơ sở dữ liệu quan hệ; tùy chọn thay SQLite cho server. |

---

## Q–Z

| Thuật ngữ (VN)   | English        | Giải thích |
|------------------|----------------|------------|
| Request / Response | Request / Response | Client gửi request (packet), server trả response (packet). |
| Repository       | Repository     | Lớp truy cập dữ liệu (đọc/ghi entity); nằm trong Infrastructure, implement interface từ Domain/Application. |
| RBAC             | RBAC           | Role-Based Access Control — phân quyền theo vai trò. |
| Server / Backend | Server / Backend | Ứng dụng console lắng nghe TCP, xử lý message và truy cập database. |
| Service Item     | Service Item   | Danh mục dịch vụ (tên, giá, đơn vị) — dùng trong RepairTask. |
| SQLite           | SQLite         | Cơ sở dữ liệu file; mặc định dùng cho server. |
| Supplier         | Supplier       | Nhà cung cấp (phụ tùng, dịch vụ). |
| Token / Session  | Token / Session | Thông tin phiên đăng nhập; client gửi kèm request để server xác thực. |
| Transaction      | Transaction    | Giao dịch thanh toán (trong bối cảnh billing); hoặc database transaction (ACID). |
| Vehicle          | Vehicle        | Xe; gắn với Customer (chủ xe). |

---

## Công nghệ

| Thuật ngữ | Giải thích |
|-----------|------------|
| **EF Core** | Entity Framework Core — ORM dùng cho truy vấn và migration. |
| **MAUI**    | .NET Multi-platform App UI — framework UI cho client (Windows, …). |
| **Nalix**   | Bộ thư viện dùng trong dự án: Nalix.Network (TCP, packet), Nalix.Logging, Nalix.Framework (config, DI). |
| **MVVM**    | Model-View-ViewModel — mô hình tổ chức UI (View ↔ ViewModel ↔ Model/Service). |

---

Nếu cần thêm thuật ngữ, có thể đề xuất qua Issue hoặc PR và cập nhật file này.
