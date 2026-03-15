# Giao thức Client–Server (Protocol)

AutoX.Gara giao tiếp **Client ↔ Server** qua **TCP**, sử dụng thư viện **Nalix.Network**. Mọi trao đổi dữ liệu theo mô hình **request–response** dựa trên **packet** (message) có OpCode và payload tuần tự hóa.

---

## 1. Tổng quan

- **Transport**: TCP (Nalix listener phía server, Nalix client phía MAUI).
- **Định dạng**: Packet với header (OpCode, flags, length, …) và vùng dữ liệu (serialize theo `LiteSerialize`).
- **Đăng ký packet**: Client và server dùng chung catalog trong `AutoX.Gara.Shared.AppConfig.Register()` (PacketRegistry).
- **Luồng**: Client gửi request (ví dụ Login, CustomerQueryRequest) → Server xử lý qua PacketDispatchChannel → Handler (Ops) → Trả response (ví dụ CustomerQueryResponse, CustomerDto).

---

## 2. Xác thực

- **Login**: Client gửi `LoginPacket` chứa `LoginRequestModel` (username, password). Server xác thực và trả response (session/token). OpCode: `OpCommand.LOGIN`.
- Các request sau **nên kèm token/session**; server dùng middleware (ví dụ PermissionMiddleware) để kiểm tra.

---

## 3. OpCode (OpCommand)

Các lệnh được định nghĩa trong `AutoX.Gara.Shared.Enums.OpCommand`:

| Nhóm | OpCode | Mô tả |
|------|--------|--------|
| **Chung** | NONE, HANDSHAKE | Không thao tác, bắt tay |
| **Auth** | LOGIN, LOGOUT, REGISTER, CHANGE_PASSWORD | Đăng nhập, đăng xuất, đổi mật khẩu |
| **Customer** | CUSTOMER_GET, CUSTOMER_CREATE, CUSTOMER_UPDATE, CUSTOMER_DELETE | CRUD khách hàng |
| **Vehicle** | VEHICLE_GET, VEHICLE_CREATE, VEHICLE_UPDATE, VEHICLE_DELETE | CRUD xe |
| **Supplier** | SUPPLIER_GET, SUPPLIER_CREATE, SUPPLIER_UPDATE, SUPPLIER_DELETE, SUPPLIER_CHANGE_STATUS | Nhà cung cấp |
| **Part** | PART_GET, PART_CREATE, PART_UPDATE, PART_DELETE | Phụ tùng (kho) |
| **Invoice** | INVOICE_GET, INVOICE_CREATE, INVOICE_UPDATE, INVOICE_DELETE | Hóa đơn |
| **RepairOrder** | REPAIR_ORDER_GET, REPAIR_ORDER_CREATE, REPAIR_ORDER_UPDATE, REPAIR_ORDER_DELETE | Đơn sửa chữa |
| **Transaction** | TRANSACTION_GET, TRANSACTION_CREATE, TRANSACTION_UPDATE, TRANSACTION_DELETE | Giao dịch thanh toán |
| **ServiceItem** | SERVICE_ITEM_GET, SERVICE_ITEM_CREATE, SERVICE_ITEM_UPDATE, SERVICE_ITEM_DELETE | Danh mục dịch vụ |
| **RepairTask** | REPAIR_TASK_GET, REPAIR_TASK_CREATE, REPAIR_TASK_UPDATE, REPAIR_TASK_DELETE | Công việc sửa chữa (trong đơn) |
| **RepairOrderItem** | REPAIR_ORDER_ITEM_GET, REPAIR_ORDER_ITEM_CREATE, REPAIR_ORDER_ITEM_UPDATE, REPAIR_ORDER_ITEM_DELETE | Hạng mục phụ tùng trong đơn |
| **Employee** | EMPLOYEE_GET, EMPLOYEE_CREATE, EMPLOYEE_UPDATE, EMPLOYEE_CHANGE_STATUS | Nhân viên |
| **EmployeeSalary** | EMPLOYEE_SALARY_GET, EMPLOYEE_SALARY_CREATE, EMPLOYEE_SALARY_UPDATE, EMPLOYEE_SALARY_DELETE | Lương nhân viên |

---

## 4. Packet types (Shared)

Các packet được đăng ký trong `AppConfig.Register()`:

- **Auth**: `LoginPacket`, `LoginRequestModel`
- **Customer**: `CustomerDto`, `CustomerQueryRequest`, `CustomerQueryResponse`
- **Vehicle**: `VehicleDto`, `VehiclesQueryResponse`
- **Employee**: `EmployeeDto`, `EmployeeQueryRequest`, `EmployeeQueryResponse`; `EmployeeSalaryDto`, `EmployeeSalaryQueryRequest`, `EmployeeSalaryQueryResponse`
- **Supplier**: `SupplierDto`, `SupplierQueryRequest`, `SupplierQueryResponse`
- **Part**: `PartDto`, `PartQueryRequest`, `PartQueryResponse`
- **Invoice**: `InvoiceDto`, `InvoiceQueryRequest`, `InvoiceQueryResponse`
- **RepairOrder**: `RepairOrderDto`, `RepairOrderQueryRequest`, `RepairOrderQueryResponse`
- **RepairOrderItem**: `RepairOrderItemDto`, `RepairOrderItemQueryRequest`, `RepairOrderItemQueryResponse`
- **RepairTask**: `RepairTaskDto`, `RepairTaskQueryRequest`, `RepairTaskQueryResponse`
- **ServiceItem**: `ServiceItemDto`, `ServiceItemQueryRequest`, `ServiceItemQueryResponse`
- **Transaction**: `TransactionDto`, `TransactionQueryRequest`, `TransactionQueryResponse`

---

## 5. Mẫu request/response (ví dụ: Customer)

- **Danh sách có phân trang**: Client gửi `CustomerQueryRequest` (Page, PageSize, SortBy, SortDescending, FilterType, FilterMembership, SearchTerm). Server trả `CustomerQueryResponse` (danh sách + tổng số).
- **Tạo/sửa**: Client gửi `CustomerDto` với OpCode tương ứng (CREATE/UPDATE), server trả response (thành công/lỗi hoặc DTO cập nhật).
- **Xóa**: Request với OpCode DELETE và id; response xác nhận.

Cấu trúc chi tiết từng DTO/Request/Response xem trong `AutoX.Gara.Shared/Protocol/`.

---

## 6. Middleware (Server)

PacketDispatchChannel trên server dùng:

- **PermissionMiddleware** — kiểm tra quyền (token/session).
- **ConcurrencyMiddleware**, **RateLimitMiddleware** — giới hạn đồng thời và tần suất.
- **UnwrapPacketMiddleware**, **WrapPacketMiddleware** — giải mã/gói packet.
- **TimeoutMiddleware** — timeout xử lý.
- Logging và error handling qua Nalix.

---

## 7. Nén (Compression)

Một số packet hỗ trợ nén chuỗi (Base64), ví dụ `LoginPacket`, `CustomerDto` có `Compress`/`Decompress` tĩnh và flag `COMPRESSED`. Client/server có thể dùng để giảm kích thước trên đường truyền.

---

## 8. Tài liệu liên quan

- [ARCHITECTURE.md](../ARCHITECTURE.md) — Luồng giao tiếp tổng quan
- `AutoX.Gara.Shared/AppConfig.cs` — Đăng ký packet
- `AutoX.Gara.Shared/Enums/OpCommand.cs` — Danh sách OpCode
- `AutoX.Gara.Application` — Handler (Ops) tương ứng từng lệnh
