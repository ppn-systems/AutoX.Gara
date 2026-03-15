# ❓ Câu hỏi thường gặp (FAQ)

---

## 🚀 Cài đặt & chạy

**Q: Tôi cần cài những gì để chạy AutoX.Gara?**  
A: .NET 10 SDK, Windows (cho client MAUI và server). Database mặc định là SQLite (không cần cài thêm). Xem [GETTING_STARTED.md](GETTING_STARTED.md).

**Q: Có chạy được trên Linux / macOS không?**  
A: Server có thể chạy trên Linux (dùng PostgreSQL). Client hiện target MAUI Windows; iOS/Mac Catalyst có trong project nhưng cần kiểm tra từng nền. Xem [COMPATIBILITY.md](COMPATIBILITY.md).

**Q: Port mặc định của server là gì?**  
A: Port do cấu hình Nalix (TCP listener) quyết định. Kiểm tra file cấu hình hoặc mã đăng ký listener trong Backend; client phải cấu hình cùng địa chỉ/port.

---

## 🗄️ Database

**Q: Có thể dùng SQL Server thay vì SQLite/PostgreSQL không?**  
A: Hiện tại Infrastructure chỉ cấu hình SQLite và PostgreSQL. Để dùng SQL Server cần thêm provider EF Core và cấu hình trong DbContext/Options.

**Q: File SQLite nằm ở đâu?**  
A: Đường dẫn nằm trong `DatabaseOptions.ConnectionString`; mặc định dùng `Directories.DatabaseDirectory` (Nalix) + `AutoX.db`. Khi chạy Backend từ IDE, thường nằm trong thư mục dữ liệu của ứng dụng (ví dụ bin/Debug/net10.0 hoặc AppData).

**Q: Làm sao đổi mật khẩu admin / tạo tài khoản mới?**  
A: Tài khoản mặc định do DataSeeder tạo (khi EnsureCreated). Có thể thêm tài khoản qua giao diện quản lý nhân viên/tài khoản (nếu đã có) hoặc tạm thời seed thêm trong code / script SQL.

---

## 📡 Giao tiếp & bảo mật

**Q: Client và server có mã hóa không?**  
A: Hiện giao tiếp TCP qua Nalix; TLS (mã hóa) cần cấu hình thêm (SslStream hoặc kênh Nalix). Khuyến nghị bật TLS khi triển khai từ xa. Xem [ARCHITECTURE.md](../ARCHITECTURE.md#7-bảo-mật-khuyến-nghị).

**Q: Mật khẩu lưu thế nào?**  
A: Server lưu dạng salted hash (không plain text). Chi tiết implementation xem trong Account/AccountOps và Infrastructure.

---

## 👨‍💻 Phát triển & đóng góp

**Q: Tôi muốn thêm một module CRUD mới.**  
A: Làm lần lượt: Domain (entity) → Infrastructure (DbContext, repository) → Shared (DTO, request/response, OpCode, đăng ký packet) → Application (Ops) → Backend (đăng ký handler) → Frontend (service, ViewModel, View). Chi tiết: [DEVELOPMENT.md](DEVELOPMENT.md).

**Q: Làm sao đóng góp code?**  
A: Fork repo, tạo branch, commit, mở Pull Request vào nhánh chính. Xem [CONTRIBUTING.md](../CONTRIBUTING.md).

**Q: Báo lỗi bảo mật ở đâu?**  
A: Không mở issue công khai. Gửi báo cáo riêng theo [SECURITY.md](../SECURITY.md).

---

## 📚 Thuật ngữ & tài liệu

**Q: OpCode, Packet, Ops là gì?**  
A: Xem [GLOSSARY.md](GLOSSARY.md) và [PROTOCOL.md](PROTOCOL.md).

**Q: Danh sách đầy đủ tài liệu?**  
A: Xem [docs/README.md](README.md) và phần “Tài liệu thêm” trong [README.md](../README.md) gốc repo.
