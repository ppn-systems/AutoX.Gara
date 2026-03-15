# 🤝 Đóng góp cho AutoX.Gara

Cảm ơn bạn quan tâm đóng góp! Tài liệu này hướng dẫn **báo lỗi**, **đề xuất tính năng** và **gửi thay đổi code**.

---

## 🐛 Báo lỗi (Bug report)

| Bước | Gợi ý |
|------|--------|
| 📌 **Mở Issue** | Repository với label phù hợp (`bug`, `documentation`, …) |
| 📝 **Mô tả** | Phiên bản .NET, OS, bước tái hiện, hành vi mong đợi vs thực tế |
| 📎 **Đính kèm** | Log hoặc ảnh chụp màn hình (nếu có) |

---

## 💡 Đề xuất tính năng

- Mở **Issue** với label `enhancement` hoặc `feature`.
- Mô tả **use case**, **lý do** cần thiết và (nếu có) **gợi ý** cách triển khai.

---

## 🔀 Gửi thay đổi code (Pull Request)

| # | Bước |
|---|------|
| 1 | **Fork** repo và clone về máy |
| 2 | Tạo **branch** mới từ `main` (vd: `fix/login-timeout`, `feat/export-invoices`) |
| 3 | Thực hiện thay đổi, tuân thủ style và cấu trúc hiện có (xem bảng Convention bên dưới) |
| 4 | **Build** và **chạy** server + client; đảm bảo không phá vỡ chức năng |
| 5 | Cập nhật tài liệu (README, ARCHITECTURE, GETTING_STARTED) nếu cần |
| 6 | **Commit** với message rõ ràng (vd: `fix: correct connection timeout`, `feat: add export CSV`) |
| 7 | **Push** branch lên fork → mở **Pull Request** vào branch chính |
| 8 | Điền mô tả PR: mục đích, cách kiểm tra, issue liên quan |

> Maintainer sẽ review; có thể yêu cầu chỉnh sửa trước khi merge.

---

## 📐 Cấu trúc code và convention

| Layer | Gợi ý |
|-------|--------|
| **Domain** | Chỉ entity, value object, logic nghiệp vụ; không reference EF Core / Infrastructure |
| **Application** | Handler message Nalix, use case (*Ops); không biết chi tiết UI/DB |
| **Infrastructure** | DbContext, Repository, Nalix listener/protocol; implement interface từ Domain/Application |
| **Shared** | DTO, request/response, config; tránh logic nghiệp vụ |

> Nếu thêm project hoặc thay đổi dependency giữa các layer → cập nhật **ARCHITECTURE.md**.

---

## ✅ Quy tắc ứng xử

Mọi đóng góp (issue, PR, thảo luận) cần tuân thủ [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

---

## ❓ Câu hỏi

Mở Issue với label `question` hoặc thảo luận trong Discussion (nếu repo bật).
