# 🚗 AutoX.Gara

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

> **AutoX.Gara** là hệ thống quản lý gara ô tô dạng **Desktop Client–Server**: quản lý khách hàng, xe, kho phụ tùng, dịch vụ sửa chữa, hóa đơn và nhân viên.  
> Client: **.NET MAUI** · Server: **.NET** · Giao tiếp: **TCP** qua [Nalix.Network](https://www.nuget.org/packages/Nalix.Network).

---

## ✨ Tính năng chính

| 🧩 Module | Mô tả |
|-----------|--------|
| 🔐 **Đăng nhập** | Xác thực tài khoản qua server, phiên làm việc an toàn |
| 👥 **Khách hàng** | CRUD khách hàng, tra cứu, lọc |
| 👔 **Nhân viên** | Quản lý hồ sơ nhân viên, lương, phân quyền |
| 🚙 **Xe (Vehicles)** | Quản lý xe gắn với khách hàng |
| 📦 **Phụ tùng (Parts)** | Quản lý kho linh kiện, tồn kho |
| 🔧 **Dịch vụ (Service Items)** | Danh mục dịch vụ sửa chữa / bảo dưỡng |
| 🏭 **Nhà cung cấp** | Quản lý nhà cung cấp phụ tùng |
| 📋 **Sửa chữa** | Đơn sửa chữa (Repair Orders), hạng mục, công việc (Repair Tasks) |
| 🧾 **Hóa đơn & Giao dịch** | Hóa đơn, giao dịch thanh toán |

---

## 📌 Yêu cầu hệ thống

| Thành phần | Yêu cầu |
|------------|---------|
| 🖥️ Runtime | **.NET 10 SDK** |
| 💻 Client | **Windows** (MAUI Windows) |
| 🗄️ Database | **SQLite** (mặc định) hoặc **PostgreSQL** (tùy chọn) |

---

## 📁 Cấu trúc solution

```
src/
├── AutoX.Gara.sln
├── AutoX.Gara.Domain/        # Entity, value object (DDD)
├── AutoX.Gara.Application/   # Use case, Nalix message handlers
├── AutoX.Gara.Infrastructure/# DbContext, Repository, Nalix listener
├── AutoX.Gara.Shared/        # DTO, protocol (request/response)
├── AutoX.Gara.Backend/       # Server (console, TCP listener)
└── AutoX.Gara.Frontend/      # Client MAUI (Windows)
```

➡️ Chi tiết: [ARCHITECTURE.md](ARCHITECTURE.md)

---

## 🚀 Build & chạy

### 1️⃣ Clone & restore

```bash
git clone https://github.com/ppn-systems/AutoX.Gara.git
cd AutoX.Gara/src
dotnet restore
```

### 2️⃣ Chạy server (Backend)

```bash
cd src/AutoX.Gara.Backend
dotnet run
```

> Server lắng nghe TCP (port theo Nalix). Database mặc định: **SQLite** (`AutoX.db`).

### 3️⃣ Chạy client (Frontend – Windows)

```bash
cd src/AutoX.Gara.Frontend
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

Hoặc mở solution trong **Visual Studio** → chọn **AutoX.Gara.Frontend** (target Windows).

### 4️⃣ Database & migrations

| Database | Ghi chú |
|---------|--------|
| **SQLite** | File DB tạo tự động (hoặc migration) |
| **PostgreSQL** | Cấu hình `DatabaseType = "PostgreSQL"` + `ConnectionString` |

```bash
cd src/AutoX.Gara.Infrastructure
dotnet ef migrations add YourMigrationName --startup-project ../AutoX.Gara.Backend
dotnet ef database update --startup-project ../AutoX.Gara.Backend
```

➡️ Hướng dẫn chi tiết: [docs/GETTING_STARTED.md](docs/GETTING_STARTED.md)

---

## 🖼️ Preview giao diện

| Màn hình | Mô tả |
|----------|--------|
| 🔐 [Đăng nhập](docs/media/login-preview.gif) | Xác thực nhanh, bảo mật |
| 👥 [Khách hàng](docs/media/customer-preview.gif) | Giao diện quản lý chuyên nghiệp |
| 👔 [Nhân viên](docs/media/employees-preview.gif) | Theo dõi, phân quyền, hồ sơ |
| 📦 [Phụ tùng](docs/media/parts-preview.gif) | Kho phụ tùng, tra cứu |
| 🔧 [Dịch vụ](docs/media/services-preview.gif) | Danh mục sửa chữa, bảo dưỡng |
| 🏭 [Nhà cung cấp](docs/media/suppliers-preview.gif) | Thông tin, lịch sử nhập hàng |

---

## 🛠️ Công nghệ sử dụng

| Thành phần | Công nghệ |
|------------|-----------|
| 🖥️ Client UI | .NET MAUI 10 (Windows) |
| ⚙️ Server | .NET 10 (Console) |
| 📡 Giao tiếp | TCP, Nalix.Network |
| 🗄️ Database | SQLite / PostgreSQL |
| 🔄 ORM | Entity Framework Core 10 |
| 📝 Logging | Nalix.Logging |
| 🧩 Client MVVM | CommunityToolkit.Mvvm |

---

## 📚 Tài liệu

<table>
<tr><td><strong>📂 Gốc repo</strong></td></tr>
<tr><td>

- [ARCHITECTURE.md](ARCHITECTURE.md) — Kiến trúc hệ thống
- [CONTRIBUTING.md](CONTRIBUTING.md) — Đóng góp code
- [CHANGELOG.md](CHANGELOG.md) — Lịch sử thay đổi
- [SECURITY.md](SECURITY.md) — Báo lỗi bảo mật
- [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) — Quy tắc ứng xử

</td></tr>
<tr><td><strong>📁 docs/</strong></td></tr>
<tr><td>

- [GETTING_STARTED](docs/GETTING_STARTED.md) · [CONFIGURATION](docs/CONFIGURATION.md) · [PROTOCOL](docs/PROTOCOL.md)
- [DEPLOYMENT](docs/DEPLOYMENT.md) · [TROUBLESHOOTING](docs/TROUBLESHOOTING.md) · [GLOSSARY](docs/GLOSSARY.md)
- [MODULES](docs/MODULES.md) · [DATABASE](docs/DATABASE.md) · [FAQ](docs/FAQ.md)
- [OPERATIONS](docs/OPERATIONS.md) · [ROADMAP](docs/ROADMAP.md) · [RELEASE](docs/RELEASE.md)
- [DEVELOPMENT](docs/DEVELOPMENT.md) · [COMPATIBILITY](docs/COMPATIBILITY.md) · [THIRD_PARTY](docs/THIRD_PARTY.md)
- [docs/README.md](docs/README.md) — Mục lục đầy đủ

</td></tr>
</table>

---

## 📄 License

Dự án dùng giấy phép **Apache-2.0**. Xem [LICENSE](LICENSE).

---

## 👤 Tác giả

**PPN Corporation**

Khi sử dụng hoặc đóng góp, vui lòng tuân thủ [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
