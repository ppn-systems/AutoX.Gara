# Lộ trình (Roadmap)

Các hướng phát triển và tính năng dự kiến cho **AutoX.Gara** (có thể điều chỉnh theo phản hồi và nguồn lực).

---

## Đã có (Hiện tại)

- Client MAUI Windows: đăng nhập, CRUD Khách hàng, Xe, Nhân viên, Phụ tùng, Dịch vụ, Nhà cung cấp, Đơn sửa chữa, Hạng mục công việc, Phụ tùng trong đơn, Hóa đơn, Giao dịch.
- Server TCP (Nalix), xác thực, phân trang, tìm kiếm, lọc.
- Database: SQLite / PostgreSQL, EF Core, migration.
- Tài liệu: README, ARCHITECTURE, GETTING_STARTED, CONFIGURATION, PROTOCOL, DEPLOYMENT, TROUBLESHOOTING, GLOSSARY, MODULES, DATABASE, FAQ, OPERATIONS, RELEASE, DEVELOPMENT, COMPATIBILITY.

---

## Ngắn hạn (Short-term)

- **TLS**: Bật mã hóa cho kết nối client–server (SslStream hoặc kênh Nalix).
- **RBAC đầy đủ**: Phân quyền theo vai trò (xem/sửa/xóa từng module), kiểm tra quyền trên mọi command.
- **Audit log**: Ghi lại đăng nhập, CRUD quan trọng (customer, invoice, repair order) phía server; xem/export log (trang admin).
- **Cải thiện tài liệu**: Thêm ví dụ cấu hình (file mẫu), video hướng dẫn cài đặt (tùy chọn).

---

## Trung hạn (Mid-term)

- **Báo cáo đơn giản**: Tổng hợp tồn kho, doanh thu theo ngày/tháng, top khách hàng (endpoint + trang client).
- **Xuất dữ liệu**: Export danh sách (Customer, Part, Invoice, …) ra CSV/Excel.
- **Localization**: Đa ngôn ngữ (VN/EN) đầy đủ, chuyển runtime.
- **Auto-update client**: Kiểm tra phiên bản mới (HTTP/HTTPS manifest), tải và cài đặt (Squirrel / MSIX / custom).

---

## Dài hạn (Long-term)

- **Offline / sync**: Cache dữ liệu trên client (SQLite/LiteDB), đồng bộ khi có mạng; xử lý xung đột (server authoritative).
- **In ấn**: In phiếu sửa, hóa đơn (template + print dialog).
- **Chấm công / Timesheet**: Module điểm danh, giờ làm (nếu cần).
- **Nền tảng mở rộng**: Client cho macOS/iOS (MAUI) nếu nhu cầu; server chạy Linux với PostgreSQL.

---

## Đóng góp ý tưởng

Nếu bạn muốn đề xuất tính năng hoặc ưu tiên, mở **Issue** với label `enhancement` hoặc `roadmap`. Xem [CONTRIBUTING.md](../CONTRIBUTING.md).
