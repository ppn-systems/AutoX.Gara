# 🚀 Hướng dẫn bắt đầu (Getting Started)

> Cài đặt môi trường, build, chạy server và client **AutoX.Gara**.

---

## 📌 Yêu cầu

| Thành phần | Ghi chú |
|------------|--------|
| 🖥️ **.NET 10 SDK** | [Tải .NET](https://dotnet.microsoft.com/download) |
| 💻 **Windows 10/11** | Client MAUI Windows + server |
| 📂 **Git** | Clone repository |

---

## 1️⃣ Clone và mở solution

```bash
git clone https://github.com/ppn-systems/AutoX.Gara.git
cd AutoX.Gara/src
```

Mở `AutoX.Gara.sln` bằng Visual Studio 2022-2026 hoặc Rider (hoặc dùng CLI như bên dưới).

---

## 2️⃣ Restore và build

```bash
cd src
dotnet restore
dotnet build
```

Nếu build Frontend cho Windows:

```bash
dotnet build AutoX.Gara.Frontend/AutoX.Gara.Frontend.csproj -f net10.0-windows10.0.19041.0
```

---

## 3️⃣ Cấu hình database (Server)

Server dùng **SQLite** mặc định (file `AutoX.db` trong thư mục dữ liệu của ứng dụng). Không cần cài đặt thêm.

Để dùng **PostgreSQL**:

1. Cấu hình trong Nalix/Backend: `DatabaseType = "PostgreSQL"` và `ConnectionString` trỏ tới instance PostgreSQL của bạn.
2. Chạy migration (xem Bước 5).

Cấu hình thường nằm trong options load bởi `ConfigurationManager` (Nalix) hoặc file cấu hình của Backend — xem `AutoX.Gara.Infrastructure.Configuration.DatabaseOptions`.

---

## 4️⃣ Chạy server (Backend)

```bash
cd src/AutoX.Gara.Backend
dotnet run
```

- Lần đầu chạy: database có thể được tạo tự động (EnsureCreated) và seed dữ liệu mẫu (nếu có).
- Port và địa chỉ lắng nghe TCP tùy cấu hình Nalix (TcpListener) — kiểm tra tài liệu Nalix hoặc cấu hình trong project Backend.

---

## 5️⃣ Chạy client (Frontend – Windows)

Mở terminal mới:

```bash
cd src/AutoX.Gara.Frontend
dotnet run -f net10.0-windows10.0.19041.0
```

Hoặc trong Visual Studio: chọn **AutoX.Gara.Frontend** làm startup project, chọn target **Windows Machine** và nhấn F5.

**Lưu ý**: Client cần cấu hình địa chỉ/port server (theo cách Frontend đọc config Nalix SDK) để kết nối đúng. Mặc định có thể là localhost — xem `AppConfig` hoặc cấu hình trong Frontend.

---

## 6️⃣ EF Core migrations (tùy chọn)

Khi thay đổi model và cần tạo/cập nhật schema:

```bash
cd src/AutoX.Gara.Infrastructure
dotnet ef migrations add TenMigration --startup-project ../AutoX.Gara.Backend
dotnet ef database update --startup-project ../AutoX.Gara.Backend
```

Design-time factory: `AutoXDbContextFactory` (đọc `DatabaseOptions` từ Nalix ConfigurationManager). Đảm bảo cấu hình DB đúng trước khi chạy migration.

---

## 🔧 Xử lý lỗi thường gặp

| Vấn đề | Gợi ý |
|--------|--------|
| Thiếu .NET 10 | Cài .NET 10 SDK; kiểm tra `dotnet --version`. |
| Frontend không build | Chọn đúng TFM: `net10.0-windows10.0.19041.0`; cần workload MAUI Windows. |
| Không kết nối được server | Kiểm tra server đã chạy; kiểm tra địa chỉ/port trong cấu hình client. |
| Lỗi database | Kiểm tra `DatabaseOptions` (ConnectionString, DatabaseType); với PostgreSQL đảm bảo server chạy và chuỗi kết nối đúng. |

---

## 📚 Tài liệu liên quan

| Tài liệu | Nội dung |
|----------|----------|
| [README.md](../README.md) | Tổng quan, tính năng, công nghệ |
| [ARCHITECTURE.md](../ARCHITECTURE.md) | Kiến trúc, lớp, luồng dữ liệu |
| [CONTRIBUTING.md](../CONTRIBUTING.md) | Đóng góp code, báo lỗi, PR |
