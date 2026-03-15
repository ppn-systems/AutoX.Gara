# 📋 Vận hành (Operations)

> Khởi động lần đầu, backup, khôi phục và theo dõi **AutoX.Gara**.

---

## 🚀 1. Chạy lần đầu (First run)

1. **Cấu hình**: Đặt DatabaseOptions (SQLite hoặc PostgreSQL) theo [CONFIGURATION.md](CONFIGURATION.md). Với SQLite, thư mục chứa file DB phải có quyền ghi.
2. **Backend**: Chạy server. Nếu dùng `EnsureCreated()`, database sẽ được tạo và DataSeeder có thể gieo tài khoản/ dữ liệu mẫu.
3. **Đăng nhập**: Dùng tài khoản do seed tạo (xem log hoặc tài liệu nội bộ) để đăng nhập từ client.
4. **Client**: Cấu hình địa chỉ/port server, chạy ứng dụng MAUI và kết nối.

Nếu dùng **migration** thay vì EnsureCreated: chạy `dotnet ef database update` trước khi start server (xem [DATABASE.md](DATABASE.md)).

---

## 💾 2. Backup

### SQLite

- **Cách đơn giản**: Sao chép file `AutoX.db` ra thư mục backup (hoặc nén với ngày trong tên, ví dụ `AutoX_2026-03-15.db.zip`).
- **Lưu ý**: Nên dừng ghi (hoặc đảm bảo không có ghi đồng thời) khi copy để tránh file hỏng. Có thể tắt server tạm thời hoặc dùng backup online của SQLite (VACUUM INTO / backup API) nếu cần.
- **Tần suất**: Hàng ngày hoặc sau mỗi phiên làm việc quan trọng; giữ vài bản theo chính sách (ví dụ 7 ngày, 4 tuần).

### PostgreSQL

- Dùng `pg_dump` (full hoặc custom format) để xuất database.
- Ví dụ: `pg_dump -h localhost -U postgres -d autox -F c -f autox_backup_20260315.dump`
- Lên lịch backup qua cron / Task Scheduler / công cụ của PostgreSQL.

### Cấu hình và secret

- Backup file cấu hình (không chứa mật khẩu plain text). Connection string nhạy cảm nên lưu trong vault hoặc biến môi trường, không đưa vào backup công khai.

---

## 🔄 3. Khôi phục (Restore)

### SQLite

- Dừng server. Thay file `AutoX.db` bằng bản backup (hoặc giải nén rồi ghi đè). Khởi động lại server.

### PostgreSQL

- Tạo database mới (nếu cần), dùng `pg_restore` với file dump.
- Cập nhật ConnectionString trỏ tới database đã restore.

Sau khi restore, kiểm tra đăng nhập và vài chức năng cơ bản.

---

## ⬆️ 4. Nâng cấp phiên bản

1. **Backup** database và cấu hình (xem mục 2).
2. **Dừng server**.
3. **Cập nhật** code/publish (pull, build, hoặc thay thư mục publish).
4. **Migration** (nếu có): `dotnet ef database update --startup-project ../AutoX.Gara.Backend` từ thư mục Infrastructure.
5. **Khởi động lại** server và kiểm tra log.
6. **Client**: Phân phối bản cài mới cho người dùng (zip, installer, hoặc cơ chế auto-update nếu có).

Chi tiết release: [RELEASE.md](RELEASE.md), triển khai: [DEPLOYMENT.md](DEPLOYMENT.md).

---

## 👀 5. Theo dõi (Monitoring)

- **Log server**: Nalix.Logging ghi file/console. Kiểm tra log khi lỗi hoặc hành vi bất thường; cấu hình level (Debug/Info/Warn/Error) và rotation theo [CONFIGURATION.md](CONFIGURATION.md).
- **Kết nối client**: Nếu client không kết nối được, kiểm tra server đang chạy, firewall, địa chỉ/port ở client.
- **Database**: Với SQLite, theo dõi kích thước file; với PostgreSQL, theo dõi kết nối, dung lượng, slow query (nếu bật).

---

## 🔧 6. Sự cố thường gặp

- **Server không start**: Xem log; thường do không kết nối được DB hoặc thiếu cấu hình. [TROUBLESHOOTING.md](TROUBLESHOOTING.md).
- **Database locked (SQLite)**: Tránh nhiều process ghi cùng lúc; cân nhắc chuyển PostgreSQL nếu nhiều client đồng thời.
- **Client không thấy dữ liệu mới**: Đảm bảo đã refresh/load lại; kiểm tra kết nối và quyền (token/session).
