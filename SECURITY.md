# 🛡️ Chính sách bảo mật (Security Policy)

---

## 🚨 Báo lỗi bảo mật

Nếu bạn phát hiện **lỗ hổng bảo mật** trong **AutoX.Gara**, vui lòng **không** mở issue công khai.

| Bước | Hành động |
|------|-----------|
| 1 | **Gửi báo cáo riêng tư** cho maintainer (email hoặc kênh liên hệ trong README / profile) |
| 2 | Mô tả rõ: **bước tái hiện**, **môi trường** (OS, .NET), **mức độ ảnh hưởng**, (nếu có) **gợi ý khắc phục** |
| 3 | Cho maintainer **thời gian hợp lý** để xác nhận và xử lý trước khi công bố công khai |

> Sau khi bản vá sẵn sàng (hoặc quyết định không vá), có thể thống nhất với người báo cáo về thời điểm công bố (CVE, advisory, CHANGELOG).

---

## 📌 Phạm vi

| Thuộc phạm vi | Không thuộc phạm vi |
|---------------|----------------------|
| Ứng dụng client (Frontend) và server (Backend) | Lỗi thư viện bên thứ ba đã có CVE công khai |
| Giao thức và cấu hình Nalix (TCP, serialization, listener) | → Có thể báo qua issue thường với label `security` |
| Cấu hình database (connection string, credentials) | |
| Xác thực/ủy quyền, lưu trữ mật khẩu/token | |

---

## ✅ Khuyến nghị khi triển khai

| Khía cạnh | Khuyến nghị |
|-----------|-------------|
| 📡 **Giao tiếp** | Dùng **TLS** cho kết nối từ xa; không gửi mật khẩu plain text |
| 🔑 **Mật khẩu** | Lưu **salted hash** (PBKDF2/Argon2) phía server |
| 🧩 **Phân quyền** | Thực thi **RBAC** và validate input **phía server** |
| 🔒 **Secret** | Connection string / secret qua biến môi trường hoặc vault; không hardcode |
| 🚫 **Đăng nhập** | Rate limiting / lockout; backup và log rotation phù hợp |

➡️ Chi tiết: [ARCHITECTURE.md](ARCHITECTURE.md#-7-bảo-mật-khuyến-nghị)
