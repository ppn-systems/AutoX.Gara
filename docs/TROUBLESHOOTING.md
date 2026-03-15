# Xử lý sự cố (Troubleshooting)

Các lỗi thường gặp khi phát triển và chạy **AutoX.Gara**, cùng cách xử lý gợi ý.

---

## 1. Build & môi trường

### Thiếu .NET 10 SDK

- **Triệu chứng**: `dotnet build` báo không tìm thấy SDK hoặc target framework `net10.0` không hợp lệ.
- **Cách xử lý**: Cài [.NET 10 SDK](https://dotnet.microsoft.com/download). Kiểm tra `dotnet --list-sdks` có bản 10.x.

### Frontend (MAUI) không build được

- **Triệu chứng**: Lỗi khi build `AutoX.Gara.Frontend` cho Windows.
- **Cách xử lý**:
  - Cài workload MAUI: `dotnet workload install maui`.
  - Chọn đúng TFM: `net10.0-windows10.0.19041.0`.
  - Build rõ ràng: `dotnet build AutoX.Gara.Frontend/AutoX.Gara.Frontend.csproj -f net10.0-windows10.0.19041.0`.
  - Visual Studio: cài đủ workload “.NET MAUI” và “Windows”.

### Restore / package lỗi

- **Triệu chứng**: Lỗi NuGet, package không restore được (Nalix, EF Core, MAUI).
- **Cách xử lý**: `dotnet restore --force`; xóa thư mục `obj`/`bin` rồi build lại. Kiểm tra nguồn NuGet (nuget.org) và phiên bản package tương thích .NET 10.

---

## 2. Database

### Không kết nối được database

- **Triệu chứng**: Server khởi động báo lỗi “Cannot connect to the database” hoặc exception từ EF Core/SQLite/Npgsql.
- **Cách xử lý**:
  - **SQLite**: Kiểm tra `DatabaseOptions.ConnectionString` trỏ tới đường dẫn file hợp lệ; thư mục chứa file phải có quyền ghi.
  - **PostgreSQL**: Kiểm tra PostgreSQL đang chạy; Host/Port/Database/Username/Password trong ConnectionString đúng; firewall không chặn port 5432.
  - Đảm bảo cấu hình được load đúng (Nalix ConfigurationManager, working directory khi chạy).

### Migration không chạy / DbContext lỗi design-time

- **Triệu chứng**: `dotnet ef migrations add` hoặc `dotnet ef database update` báo lỗi hoặc không tìm thấy DbContext.
- **Cách xử lý**:
  - Chạy từ thư mục `AutoX.Gara.Infrastructure`, startup project là `AutoX.Gara.Backend`:  
    `dotnet ef migrations add TenMigration --startup-project ../AutoX.Gara.Backend`
  - Design-time factory `AutoXDbContextFactory` cần đọc được `DatabaseOptions` (ConfigurationManager). Đảm bảo Nalix load config khi chạy từ thư mục Infrastructure (thường qua startup project Backend).
  - Nếu dùng PostgreSQL, đảm bảo server chạy và connection string đúng khi gọi `ef`.

### Database bị khóa (SQLite)

- **Triệu chứng**: SQLite “database is locked” hoặc timeout khi nhiều thao tác.
- **Cách xử lý**: Tránh mở nhiều process ghi cùng file; tăng CommandTimeout trong cấu hình EF (nếu cần); cân nhắc chuyển sang PostgreSQL cho nhiều client đồng thời.

---

## 3. Kết nối Client – Server

### Client không kết nối được server

- **Triệu chứng**: Client báo lỗi kết nối, timeout, hoặc không đăng nhập được.
- **Cách xử lý**:
  - Kiểm tra server đã chạy và listener đang mở (log khi start).
  - Kiểm tra địa chỉ/port cấu hình ở client (localhost vs IP, port đúng với Nalix listener).
  - Firewall Windows (và firewall mạng) cho phép port TCP ra/vào.
  - Thử từ cùng máy (client và server cùng localhost) trước khi thử từ xa.

### Đăng nhập thất bại

- **Triệu chứng**: Gửi login nhưng không vào được (sai mật khẩu / tài khoản khóa).
- **Cách xử lý**: Kiểm tra tài khoản đã được seed (DataSeeder); so sánh hash mật khẩu phía server. Xem log server khi login (không log mật khẩu plain text).

### Packet / serialization lỗi

- **Triệu chứng**: Server hoặc client báo lỗi khi nhận/gửi packet (deserialize, OpCode không nhận dạng).
- **Cách xử lý**: Client và server phải dùng chung catalog packet (`AppConfig.Register()`); cùng phiên bản Shared và Nalix. Kiểm tra OpCode và thứ tự field serialize (SerializeOrder) khớp giữa hai bên.

---

## 4. Runtime

### Server thoát ngay sau khi chạy

- **Triệu chứng**: Console mở rồi đóng ngay hoặc báo exception.
- **Cách xử lý**: Chạy từ terminal để xem log/stack trace. Thường do: không load được cấu hình (DatabaseOptions, Nalix), lỗi kết nối DB, hoặc exception trong đăng ký dependency / listener. Sửa cấu hình hoặc dependency injection theo log.

### Client crash khi mở màn hình

- **Triệu chứng**: Ứng dụng MAUI đóng khi vào một trang hoặc thao tác nào đó.
- **Cách xử lý**: Gỡ lỗi bằng Visual Studio (F5, xem exception); kiểm tra ViewModel gọi service/API với dữ liệu null hoặc chưa kết nối server; kiểm tra binding XAML và ObservableCollection/threading.

### Hiệu năng chậm (danh sách lớn)

- **Triệu chứng**: Trang danh sách (customer, part, …) load lâu hoặc lag.
- **Cách xử lý**: Dùng phân trang (Page, PageSize) trong request; tránh load toàn bộ dữ liệu một lần. Kiểm tra query phía server (index, N+1) và log thời gian phản hồi.

---

## 5. Nơi lấy thêm thông tin

- **Server**: Nalix.Logging (file/console); level Debug khi cần chi tiết.
- **Client**: Debugger (Visual Studio); log trong Frontend nếu có.
- **EF Core**: Có thể bật logging SQL (sensitive data chỉ dùng khi debug, không bật trên production).

Nếu lỗi không nằm trong danh sách trên, mở **Issue** trên repository với mô tả bước tái hiện, log và môi trường (OS, .NET version). Xem [CONTRIBUTING.md](../CONTRIBUTING.md).
