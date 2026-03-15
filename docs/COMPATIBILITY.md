# Tương thích (Compatibility)

Thông tin về nền tảng và phiên bản được hỗ trợ cho **AutoX.Gara**.

---

## 1. Runtime & SDK

| Thành phần | Yêu cầu |
|------------|---------|
| **.NET** | .NET 10.0 (SDK để build, Runtime để chạy). |
| **Backend** | net10.0 (console). Có thể publish self-contained (win-x64, linux-x64, …). |
| **Frontend** | net10.0-windows10.0.19041.0 (MAUI Windows). Các target khác (iOS, Mac Catalyst) có trong csproj nhưng cần kiểm tra từng nền. |

Kiểm tra phiên bản: `dotnet --version` (cần 10.x).

---

## 2. Hệ điều hành

| Thành phần | Hệ điều hành |
|------------|---------------|
| **Server (Backend)** | Windows (đã kiểm tra). Linux khi dùng .NET 10 + PostgreSQL (cần tự kiểm tra Nalix trên Linux). |
| **Client (Frontend)** | Windows 10/11 (MAUI Windows). Cấu hình tối thiểu: TargetPlatformMinVersion 10.0.17763.0. |

---

## 3. Database

| Database | Phiên bản (gợi ý) | Ghi chú |
|----------|--------------------|---------|
| **SQLite** | 3.x (qua EF Core provider) | Mặc định; file .db, không cần cài riêng. |
| **PostgreSQL** | 12+ (qua Npgsql) | Cấu hình DatabaseType = "PostgreSQL" và ConnectionString. |

EF Core: 10.0.x; Npgsql.EntityFrameworkCore.PostgreSQL: 10.x.

---

## 4. Thư viện bên thứ ba (chính)

- **Nalix.SDK**, **Nalix.Network**, **Nalix.Logging**, **Nalix.Framework**, **Nalix.Common**, **Nalix.Shared** — phiên bản theo package reference trong từng project (ví dụ 11.3.0).
- **Microsoft.EntityFrameworkCore** 10.0.x.
- **CommunityToolkit.Mvvm** — phiên bản theo Frontend csproj.
- **Microsoft.Maui.Controls** 10.x.

Nâng cấp package: kiểm tra release notes và breaking change; chạy build và test sau khi cập nhật.

---

## 5. Trình duyệt / Thiết bị

- Client là **ứng dụng desktop MAUI**, không chạy trên trình duyệt.
- Khuyến nghị độ phân giải màn hình tối thiểu cho giao diện (theo thiết kế MAUI); không có yêu cầu trình duyệt.

---

## 6. Thay đổi tương thích

- **Phiên bản major** (x.y.z, tăng x): Có thể thay đổi API, protocol hoặc schema không tương thích ngược; ghi trong CHANGELOG.
- **Migration EF**: Khi thêm/sửa migration, phiên bản cũ của app có thể không chạy được trên DB mới (hoặc ngược lại) — cần quy trình nâng cấp và backup. Xem [DATABASE.md](DATABASE.md), [OPERATIONS.md](OPERATIONS.md).

Nếu bạn cần hỗ trợ nền tảng cụ thể (ví dụ Linux server, macOS client), có thể mở Issue với label `compatibility` hoặc `enhancement`.
