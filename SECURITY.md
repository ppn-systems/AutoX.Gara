# Chính sách bảo mật (Security Policy)

## Báo lỗi bảo mật

Nếu bạn phát hiện lỗ hổng bảo mật trong **AutoX.Gara**, vui lòng **không** mở issue công khai. Thay vào đó:

1. **Gửi báo cáo riêng tư** cho maintainer qua email hoặc kênh liên hệ được công bố trong repository (ví dụ: trong README hoặc profile tổ chức).
2. Mô tả rõ: bước tái hiện, môi trường (OS, .NET version), mức độ ảnh hưởng (impact) và (nếu có) gợi ý cách khắc phục.
3. Cho maintainer thời gian hợp lý để xác nhận và xử lý trước khi công bố công khai.

Chúng tôi sẽ cố gắng xác nhận và phản hồi trong thời gian sớm nhất. Sau khi bản vá đã sẵn sàng (hoặc quyết định không vá), có thể thống nhất với người báo cáo về thời điểm công bố (CVE, advisory, CHANGELOG).

## Phạm vi

- Ứng dụng client (AutoX.Gara.Frontend) và server (AutoX.Gara.Backend).
- Giao thức và cấu hình Nalix (TCP, serialization, listener).
- Cấu hình database (connection string, credentials).
- Cơ chế xác thực/ủy quyền và lưu trữ mật khẩu/token.

Các vấn đề không thuộc phạm vi (ví dụ: lỗi của thư viện bên thứ ba đã có CVE công khai) vẫn có thể báo qua issue thông thường với label `security` nếu liên quan đến cách dự án sử dụng thư viện đó.

## Khuyến nghị khi triển khai

- Dùng **TLS** cho mọi kết nối client–server từ xa; không gửi mật khẩu dạng plain text.
- Lưu mật khẩu dưới dạng **salted hash** (PBKDF2/Argon2) phía server.
- Thực thi **RBAC** và validate input **phía server**; không tin dữ liệu từ client.
- Cấu hình **connection string** và secret qua biến môi trường hoặc vault, không hardcode trong repo.
- Bật **rate limiting / lockout** cho đăng nhập; thiết lập **backup** và **log rotation** phù hợp.

Chi tiết kiến trúc bảo mật: [ARCHITECTURE.md](ARCHITECTURE.md#7-bảo-mật-khuyến-nghị).
