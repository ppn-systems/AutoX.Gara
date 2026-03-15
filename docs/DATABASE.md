# 🗄️ Cơ sở dữ liệu (Database)

> Tổng quan **schema** và cách làm việc với database trong AutoX.Gara.

---

## 🛠️ 1. Công nghệ

- **ORM**: Entity Framework Core 10.
- **Provider**: SQLite (mặc định) hoặc PostgreSQL (cấu hình).
- **Cách tạo schema**: Code First — cấu hình trong `AutoXDbContext`, migration trong `AutoX.Gara.Infrastructure/Migrations/`.
- **Design-time**: `AutoXDbContextFactory` (đọc `DatabaseOptions` từ Nalix ConfigurationManager).

---

## 📋 2. Bảng (Entities / DbSet)

| Bảng (DbSet) | Entity | Mô tả ngắn |
|--------------|--------|------------|
| Accounts | Account | Tài khoản đăng nhập (username, password hash, liên kết Employee) |
| Employees | Employee | Nhân viên (tên, chức vụ, trạng thái, AccountId) |
| EmployeeSalaries | EmployeeSalary | Lịch sử lương nhân viên (EmployeeId, mức lương, thời hiệu lực) |
| Customers | Customer | Khách hàng (tên, email, SĐT, địa chỉ, loại, hạng thành viên) |
| Vehicles | Vehicle | Xe (CustomerId, biển số, hiệu, model, màu) |
| Suppliers | Supplier | Nhà cung cấp |
| SupplierContactPhones | SupplierContactPhone | Số điện thoại liên hệ nhà cung cấp |
| Parts | Part | Phụ tùng (mã, tên, số lượng, giá, SupplierId, …) |
| ServiceItems | ServiceItem | Danh mục dịch vụ (mô tả, loại, giá) |
| Invoices | Invoice | Hóa đơn (CustomerId, ngày, trạng thái, tổng tiền) |
| RepairOrders | RepairOrder | Đơn sửa chữa (VehicleId, CustomerId, InvoiceId, ngày, trạng thái) |
| RepairTasks | RepairTask | Công việc trong đơn (RepairOrderId, ServiceItemId, EmployeeId) |
| RepairOrderItems | RepairOrderItem | Phụ tùng dùng trong đơn (RepairOrderId, PartId, số lượng, đơn giá) |
| Transactions | Transaction | Giao dịch thanh toán (InvoiceId, số tiền, phương thức, trạng thái) |

---

## 🔗 3. Quan hệ chính

```
Account 1 ────── 1 Employee
Customer 1 ────── N Vehicle
Customer 1 ────── N Invoice
Invoice  1 ────── 1 RepairOrder   (unique)
Invoice  1 ────── N Transaction
RepairOrder N ─── 1 Vehicle
RepairOrder 1 ─── N RepairTask    (ServiceItem, Employee)
RepairOrder 1 ─── N RepairOrderItem (Part)
Supplier 1 ────── N Part
Employee 1 ────── N EmployeeSalary
Supplier 1 ────── N SupplierContactPhone
```

- **RepairOrder** nối Customer, Vehicle, Invoice và chứa RepairTask (dịch vụ) + RepairOrderItem (phụ tùng).
- **Transaction** gắn với Invoice (thanh toán cho hóa đơn).

---

## 📌 4. Index

Một số index đã cấu hình trong `AutoXDbContext` (ví dụ):

- **Customer**: Name, Email, PhoneNumber, TaxCode.
- **Vehicle**: LicensePlate, CustomerId, …
- **Part**: PartName, PartCode, Manufacturer, SupplierId.
- **Invoice**: CustomerId, InvoiceDate.
- **RepairOrder**: VehicleId, CustomerId, InvoiceId (unique).
- **Employee**: Name, Position, Status, StartDate.
- **Transaction**: InvoiceId, Status, Type, TransactionDate.
- **RepairTask**: Status, EmployeeId, (StartDate, CompletionDate).
- **EmployeeSalary**: EmployeeId, EffectiveFrom, EffectiveTo.

Chi tiết đầy đủ xem trong `AutoXDbContext.OnModelCreating`.

---

## 🔄 5. Migration

### Tạo migration mới

```bash
cd src/AutoX.Gara.Infrastructure
dotnet ef migrations add TenMigrationName --startup-project ../AutoX.Gara.Backend
```

### Cập nhật database

```bash
dotnet ef database update --startup-project ../AutoX.Gara.Backend
```

### Rollback

```bash
dotnet ef database update PreviousMigrationName --startup-project ../AutoX.Gara.Backend
```

**Lưu ý**: Cấu hình `DatabaseOptions` (ConnectionString, DatabaseType) phải đúng khi chạy; design-time factory dùng ConfigurationManager của Nalix (load qua startup project Backend).

---

## 🌱 6. Khởi tạo lần đầu (EnsureCreated & Seed)

Backend khi chạy lần đầu có thể gọi `context.Database.EnsureCreated()` — nếu database chưa tồn tại sẽ tạo schema theo model hiện tại (không dùng file migration). Sau đó `DataSeeder.SeedAsync(context)` gieo dữ liệu mẫu (ví dụ tài khoản admin).

- **Production**: Nên dùng **migration** thay vì EnsureCreated để kiểm soát thay đổi schema theo version.
- **Backup**: Trước khi chạy migration trên DB có dữ liệu, nên backup (file SQLite hoặc dump PostgreSQL).

---

## 📝 7. Connection string mẫu

- **SQLite**: `Data Source=C:\Path\To\AutoX.db`
- **PostgreSQL**: `Host=localhost;Port=5432;Database=autox;Username=postgres;Password=***`

Cấu hình: [CONFIGURATION.md](CONFIGURATION.md).
