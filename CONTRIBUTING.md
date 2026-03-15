# Đóng góp cho AutoX.Gara

Cảm ơn bạn quan tâm đóng góp cho **AutoX.Gara**. Tài liệu này hướng dẫn cách báo lỗi, đề xuất tính năng và gửi thay đổi code.

---

## Báo lỗi (Bug report)

- Mở **Issue** trên repository với label phù hợp (bug, documentation, …).
- Mô tả rõ: phiên bản .NET, OS, bước tái hiện, hành vi mong đợi vs thực tế.
- Nếu có thể, đính kèm log hoặc ảnh chụp màn hình.

---

## Đề xuất tính năng

- Mở **Issue** với label `enhancement` hoặc `feature`.
- Mô tả use case, lý do cần thiết và (nếu có) gợi ý cách triển khai.

---

## Gửi thay đổi code (Pull Request)

1. **Fork** repository và clone về máy.
2. Tạo **branch** mới từ `main` (hoặc `develop` nếu repo dùng flow đó), đặt tên rõ ràng (ví dụ: `fix/login-timeout`, `feat/export-invoices`).
3. Thực hiện thay đổi, tuân thủ style và cấu trúc hiện có:
   - C#: format nhất quán, đặt tên rõ ràng.
   - Domain: giữ Domain layer không phụ thuộc EF/Infrastructure.
   - Application: use case trong các class *Ops, gọi Repository/Domain.
4. **Build** và **chạy** (server + client) để đảm bảo không phá vỡ chức năng.
5. Cập nhật tài liệu (README, ARCHITECTURE, GETTING_STARTED) nếu thay đổi ảnh hưởng tới cách dùng hoặc kiến trúc.
6. **Commit** với message rõ ràng (ví dụ: `fix: correct connection timeout handling`, `feat: add export to CSV for invoices`).
7. **Push** branch lên fork và mở **Pull Request** vào branch chính của repo gốc.
8. Điền mô tả PR: mục đích, cách kiểm tra, issue liên quan (nếu có).

Maintainer sẽ review và phản hồi. Có thể cần chỉnh sửa thêm trước khi merge.

---

## Quy tắc ứng xử

Mọi đóng góp (issue, PR, thảo luận) cần tuân thủ [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

---

## Cấu trúc code và convention

- **Domain**: Chỉ entity, value object, logic nghiệp vụ thuần; không reference EF Core hay Infrastructure.
- **Application**: Handler message Nalix, orchestration use case; không biết chi tiết UI hay DB.
- **Infrastructure**: DbContext, Repository, Nalix listener/protocol; implement interface từ Domain/Application khi cần.
- **Shared**: DTO, request/response, config dùng chung; tránh logic nghiệp vụ.

Nếu thêm project hoặc thay đổi dependency giữa các layer, cập nhật **ARCHITECTURE.md** cho đúng.

---

## Câu hỏi

Nếu có câu hỏi về cách đóng góp, mở Issue với label `question` hoặc thảo luận trong Discussion (nếu repo bật tính năng đó).
