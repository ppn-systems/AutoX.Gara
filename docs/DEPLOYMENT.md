# 🚀 Triển khai (Deployment)

> Đóng gói và triển khai **Backend** (server) và **Frontend** (client) AutoX.Gara.

---

## 📌 1. Yêu cầu môi trường

- **Server**: Windows (hoặc Linux nếu dùng .NET 10 cho console + PostgreSQL).
- **Client**: Windows 10/11 (MAUI Windows).
- **.NET 10 Runtime** (hoặc self-contained publish không cần cài runtime).

---

## ⚙️ 2. Backend (Server)

### 📤 2.1 Publish

Từ thư mục solution:

```bash
cd src
dotnet publish AutoX.Gara.Backend/AutoX.Gara.Backend.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/backend
```

- `-r win-x64`: bản Windows 64-bit. Có thể dùng `linux-x64` nếu chạy trên Linux.
- `--self-contained true`: gói kèm .NET runtime, máy đích không cần cài .NET.
- `PublishSingleFile=true`: tạo 1 file exe (đã cấu hình trong .csproj).

### ⚙️ 2.2 Cấu hình khi triển khai

- Đặt **DatabaseOptions** (DatabaseType, ConnectionString) đúng môi trường: file cấu hình Nalix hoặc biến môi trường (nếu Nalix hỗ trợ bind).
- **SQLite**: đảm bảo thư mục chứa file `.db` có quyền ghi; backup định kỳ file DB.
- **PostgreSQL**: cấu hình ConnectionString (Host, Port, Database, User, Password); không lưu mật khẩu trong repo.

### 🔧 2.3 Chạy như dịch vụ (Windows)

- Dùng **Windows Service** hoặc **NSSM** / **sc.exe** để cài server chạy nền, khởi động cùng máy.
- Hoặc chạy trong console và dùng công cụ giám sát (PM2 trên Node không áp dụng trực tiếp; có thể dùng task scheduler hoặc wrapper).

### 🔒 2.4 Firewall & mạng

- Mở **port TCP** mà Nalix listener đang lắng nghe (xem cấu hình Nalix).
- Nếu client kết nối từ xa: bật TLS (SslStream hoặc kênh Nalix) và chỉ mở port trên interface cần thiết.

---

## 🖥️ 3. Frontend (Client MAUI Windows)

### 3.1 Publish

```bash
cd src
dotnet publish AutoX.Gara.Frontend/AutoX.Gara.Frontend.csproj -c Release -f net10.0-windows10.0.19041.0 -o ./publish/frontend
```

- Thư mục `publish/frontend` chứa exe và các file phụ thuộc (hoặc dùng self-contained để gói gọn hơn).

Self-contained (một thư mục đầy đủ):

```bash
dotnet publish AutoX.Gara.Frontend/AutoX.Gara.Frontend.csproj -c Release -f net10.0-windows10.0.19041.0 -r win-x64 --self-contained true -o ./publish/frontend
```

### 3.2 Cấu hình client khi triển khai

- **Địa chỉ/port server**: cấu hình Nalix client (hoặc config file) trỏ tới server thực tế (IP/hostname, port).
- Có thể phân phối file cấu hình kèm bản cài hoặc để người dùng nhập lần đầu.

### 3.3 Phân phối cho người dùng

- **Cách 1**: Nén thư mục publish (zip) và gửi; người dùng giải nén và chạy exe.
- **Cách 2**: Đóng gói **MSIX** (Store hoặc sideload) — cần cấu hình thêm trong project MAUI.
- **Cách 3**: Installer (Inno Setup, WiX, etc.) tạo bộ cài từ thư mục publish.

---

## 📋 4. Thứ tự triển khai

1. **Database**: Tạo DB (PostgreSQL) hoặc đảm bảo thư mục SQLite có quyền ghi; chạy migration nếu dùng migration thay vì EnsureCreated.
2. **Backend**: Cấu hình DatabaseOptions và (nếu có) Nalix listener; copy bản publish; chạy và kiểm tra log.
3. **Firewall**: Mở port cho TCP listener.
4. **Frontend**: Cấu hình địa chỉ server; phân phối bản publish hoặc installer cho từng máy client.
5. **Kiểm tra**: Đăng nhập từ client, thao tác CRUD cơ bản để xác nhận kết nối và quyền.

---

## 💾 5. Backup & nâng cấp

- **Backup**: Sao lưu định kỳ file SQLite hoặc dump PostgreSQL; lưu cấu hình (không lưu mật khẩu dạng plain text).
- **Nâng cấp**: Dừng server → backup DB → thay file publish mới → chạy migration (nếu có) → khởi động lại. Client: phát bản mới (zip/installer/auto-update) và hướng dẫn người dùng cập nhật.

Xem thêm: [CONFIGURATION.md](CONFIGURATION.md), [GETTING_STARTED.md](GETTING_STARTED.md).
