# Cấu hình (Configuration)

Tài liệu tham khảo cấu hình cho **AutoX.Gara** Backend và môi trường chạy.

---

## 1. Database (Server)

Backend đọc cấu hình qua **Nalix ConfigurationManager**. Class cấu hình: `AutoX.Gara.Infrastructure.Configuration.DatabaseOptions`.

| Thuộc tính | Kiểu | Mặc định | Mô tả |
|------------|------|----------|--------|
| **DatabaseType** | string | `"SQLite"` | Loại DB: `SQLite` hoặc `PostgreSQL` (phân biệt chữ hoa/thường). |
| **ConnectionString** | string | `Data Source=<DatabaseDirectory>/AutoX.db` | Chuỗi kết nối. SQLite: đường dẫn file `.db`. PostgreSQL: chuỗi chuẩn Npgsql (Host, Port, Database, Username, Password, …). |

### SQLite (mặc định)

- `Directories.DatabaseDirectory` thường do Nalix quy định (thư mục dữ liệu ứng dụng).
- Ví dụ: `Data Source=C:\AppData\AutoX\AutoX.db`.
- Không cần cài đặt thêm; file DB được tạo tự động khi chạy lần đầu (EnsureCreated) hoặc qua migration.

### PostgreSQL

- Đặt `DatabaseType = "PostgreSQL"`.
- Ví dụ ConnectionString:  
  `Host=localhost;Port=5432;Database=autox;Username=postgres;Password=***`
- Cần cài và chạy PostgreSQL; tạo database trước khi chạy migration hoặc EnsureCreated.
- EF Core: bảng migration history: `__MigrationsHistory` (schema `public` cho Npgsql).

---

## 2. Logging (Server)

Backend dùng **Nalix.Logging**. Cấu hình qua `NLogixOptions` (Nalix), ví dụ:

- **MinLevel**: mức log tối thiểu (Debug, Info, Warn, Error).
- Các target (file, console) do code đăng ký trong `Program.cs` (ví dụ `BatchFileLogTarget`).

Chi tiết tùy phiên bản Nalix.Logging và NLogixOptions — xem tài liệu Nalix hoặc mã trong `Program.cs`.

---

## 3. Network (TCP Listener)

Server dùng **AutoXListener** (kế thừa Nalix `TcpListenerBase`). Địa chỉ và port lắng nghe do **Nalix** cấu hình (options của listener/transport). Kiểm tra:

- Tài liệu Nalix.Network cho TCP listener.
- Mã khởi tạo `AutoXListener` và đăng ký `IListener` trong `Program.cs`.
- Biến môi trường hoặc file cấu hình mà Nalix đọc khi khởi động.

Client (Frontend) cần cấu hình **địa chỉ và port** của server để kết nối — thường qua Nalix SDK client options hoặc file cấu hình của ứng dụng MAUI.

---

## 4. Client (Frontend)

- **Server address/port**: cấu hình kết nối tới Backend (Nalix client).
- **Ngôn ngữ / localization**: nếu có, thường qua resource (.resx) hoặc file cấu hình.
- Các tùy chọn khác (timeout, retry) tùy Nalix.SDK và mã Frontend.

---

## 5. Nơi đặt cấu hình

- **Nalix**: cấu hình thường load từ file (JSON/XML), biến môi trường hoặc code, tùy cách bạn đăng ký với `ConfigurationManager`.
- **DatabaseOptions**: đảm bảo được bind vào `ConfigurationManager.Instance.Get<DatabaseOptions>()` (trong Backend/Infrastructure) để DbContext factory hoạt động đúng.
- **Bảo mật**: không commit connection string có mật khẩu vào Git; dùng biến môi trường hoặc secret store trong môi trường production.

---

## 6. Design-time (EF Core migrations)

Khi chạy `dotnet ef migrations` hoặc `dotnet ef database update`, **AutoXDbContextFactory** dùng cùng `DatabaseOptions` (qua ConfigurationManager). Đảm bảo:

- Working directory hoặc cấu hình Nalix trỏ đúng khi chạy từ thư mục `AutoX.Gara.Infrastructure` với startup project `AutoX.Gara.Backend`.
- Nếu dùng PostgreSQL, instance phải chạy và ConnectionString đúng để migration kết nối được.

Xem thêm: [GETTING_STARTED.md](GETTING_STARTED.md#bước-6-ef-core-migrations-tùy-chọn).
