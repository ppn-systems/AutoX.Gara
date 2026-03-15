# Tổng quan module (Modules Overview)

Tài liệu mô tả từng **module chức năng** của AutoX.Gara: entity, màn hình client, handler server (Ops) và mục đích sử dụng.

---

## 1. Identity (Đăng nhập & Nhân sự)

**Mục đích**: Xác thực người dùng, quản lý tài khoản và nhân viên.

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `Account` (đăng nhập), `Employee` (hồ sơ nhân viên), `EmployeeSalary` (lương) |
| **Client** | LoginPage, EmployeesPage (danh sách/sửa nhân viên); có thể có màn hình lương |
| **Server Ops** | `AccountOps`, `EmployeeOps`, `EmployeeSalaryOps` |
| **OpCode** | LOGIN, LOGOUT; EMPLOYEE_GET/CREATE/UPDATE/CHANGE_STATUS; EMPLOYEE_SALARY_* |

**Ghi chú**: Account gắn với Employee; phân quyền (RBAC) có thể mở rộng sau.

---

## 2. Customer (Khách hàng)

**Mục đích**: Quản lý thông tin khách hàng (cá nhân/doanh nghiệp), hạng thành viên.

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `Customer` (Name, Email, Phone, Address, TaxCode, Type, Membership, …) |
| **Client** | CustomersPage — danh sách, tìm kiếm, lọc, tạo/sửa/xóa |
| **Server Ops** | `CustomerOps` |
| **OpCode** | CUSTOMER_GET, CUSTOMER_CREATE, CUSTOMER_UPDATE, CUSTOMER_DELETE |

**Shared**: `CustomerDto`, `CustomerQueryRequest`, `CustomerQueryResponse`; sort/filter theo CustomerSortField, CustomerType, MembershipLevel.

---

## 3. Vehicle (Xe)

**Mục đích**: Quản lý xe gắn với khách hàng (biển số, hiệu xe, màu, …).

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `Vehicle` (CustomerId, LicensePlate, Brand, Model, Color, …) |
| **Client** | VehiclesPage — danh sách xe, CRUD, gắn với khách hàng |
| **Server Ops** | `VehicleOps` |
| **OpCode** | VEHICLE_GET, VEHICLE_CREATE, VEHICLE_UPDATE, VEHICLE_DELETE |

**Quan hệ**: Vehicle thuộc một Customer. RepairOrder gắn Vehicle (và Customer).

---

## 4. Inventory – Part & Supplier (Kho & Nhà cung cấp)

**Mục đích**: Quản lý kho phụ tùng (Part) và nhà cung cấp (Supplier).

### Part (Phụ tùng)

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `Part` (PartCode, PartName, Quantity, Price, Category, SupplierId, …) |
| **Client** | PartsPage — danh sách, tìm kiếm, CRUD |
| **Server Ops** | `SparePartOps` (PartOps) |
| **OpCode** | PART_GET, PART_CREATE, PART_UPDATE, PART_DELETE |

### Supplier (Nhà cung cấp)

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `Supplier`, `SupplierContactPhone` |
| **Client** | SuppliersPage — danh sách nhà cung cấp, CRUD, trạng thái |
| **Server Ops** | `SupplierOps` |
| **OpCode** | SUPPLIER_GET, SUPPLIER_CREATE, SUPPLIER_UPDATE, SUPPLIER_DELETE, SUPPLIER_CHANGE_STATUS |

**Quan hệ**: Part có thể gắn Supplier. RepairOrderItem tham chiếu Part (phụ tùng dùng trong đơn sửa).

---

## 5. Billing – Service Item, Invoice, Transaction

**Mục đích**: Danh mục dịch vụ, hóa đơn và giao dịch thanh toán.

### Service Item (Dịch vụ)

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `ServiceItem` (mô tả, loại, đơn giá, …) |
| **Client** | ServiceItemsPage — danh mục dịch vụ sửa chữa/bảo dưỡng |
| **Server Ops** | `ServiceItemOps` |
| **OpCode** | SERVICE_ITEM_GET, SERVICE_ITEM_CREATE, SERVICE_ITEM_UPDATE, SERVICE_ITEM_DELETE |

### Invoice (Hóa đơn)

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `Invoice` (CustomerId, InvoiceDate, trạng thái, tổng tiền, …) |
| **Client** | InvoicesPage, InvoicesOverviewPage — danh sách hóa đơn, tạo/sửa |
| **Server Ops** | `InvoiceOps` |
| **OpCode** | INVOICE_GET, INVOICE_CREATE, INVOICE_UPDATE, INVOICE_DELETE |

### Transaction (Giao dịch thanh toán)

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `Transaction` (InvoiceId, số tiền, phương thức, trạng thái, …) |
| **Client** | TransactionsPage — giao dịch theo hóa đơn |
| **Server Ops** | `TransactionOps` |
| **OpCode** | TRANSACTION_GET, TRANSACTION_CREATE, TRANSACTION_UPDATE, TRANSACTION_DELETE |

**Quan hệ**: Invoice thuộc Customer. RepairOrder gắn Invoice (1-1). Transaction thuộc Invoice.

---

## 6. Repair (Sửa chữa)

**Mục đích**: Đơn sửa chữa (RepairOrder), công việc (RepairTask), phụ tùng dùng trong đơn (RepairOrderItem).

### Repair Order (Đơn sửa chữa)

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `RepairOrder` (VehicleId, CustomerId, InvoiceId, OrderDate, Status, …) |
| **Client** | RepairOrdersPage — danh sách đơn, tạo/sửa, xem chi tiết |
| **Server Ops** | `RepairOrderOps` |
| **OpCode** | REPAIR_ORDER_GET, REPAIR_ORDER_CREATE, REPAIR_ORDER_UPDATE, REPAIR_ORDER_DELETE |

### Repair Task (Công việc sửa chữa)

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `RepairTask` (RepairOrderId, ServiceItemId, EmployeeId, thời gian, trạng thái, …) |
| **Client** | RepairTasksPage — hạng mục công việc trong đơn |
| **Server Ops** | `RepairTaskOps` |
| **OpCode** | REPAIR_TASK_GET, REPAIR_TASK_CREATE, REPAIR_TASK_UPDATE, REPAIR_TASK_DELETE |

### Repair Order Item (Phụ tùng trong đơn)

| Thành phần | Mô tả |
|-------------|--------|
| **Entity** | `RepairOrderItem` (RepairOrderId, PartId, số lượng, đơn giá, …) |
| **Client** | RepairOrderItemsPage — phụ tùng sử dụng trong từng đơn |
| **Server Ops** | `RepairOrderItemOps` |
| **OpCode** | REPAIR_ORDER_ITEM_GET, REPAIR_ORDER_ITEM_CREATE, REPAIR_ORDER_ITEM_UPDATE, REPAIR_ORDER_ITEM_DELETE |

**Quan hệ**: RepairOrder → nhiều RepairTask (ServiceItem), nhiều RepairOrderItem (Part). RepairOrder gắn Vehicle, Customer, Invoice.

---

## 7. Communication (Giao tiếp hệ thống)

| Thành phần | Mô tả |
|-------------|--------|
| **Ops** | `PingOps`, `HandshakeOps` — kiểm tra kết nối, bắt tay |
| **OpCode** | HANDSHAKE, (Ping thường dùng OpCode riêng hoặc nội bộ) |

---

## Bản đồ màn hình (Client)

| Trang | Module chính |
|-------|---------------|
| LoginPage | Identity |
| MainPage | Shell / điều hướng |
| CustomersPage | Customer |
| VehiclesPage | Vehicle |
| EmployeesPage | Identity (Employee) |
| PartsPage | Part |
| SuppliersPage | Supplier |
| ServiceItemsPage | ServiceItem |
| RepairOrdersPage | RepairOrder |
| RepairTasksPage | RepairTask |
| RepairOrderItemsPage | RepairOrderItem |
| InvoicesPage / InvoicesOverviewPage | Invoice |
| TransactionsPage | Transaction |

Chi tiết entity và quan hệ: [Architecture.md](Architecture.md), [DATABASE.md](DATABASE.md). Giao thức request/response: [PROTOCOL.md](PROTOCOL.md).
