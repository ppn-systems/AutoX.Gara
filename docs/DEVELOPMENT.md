# 👨‍💻 Hướng dẫn phát triển (Development)

> Quy trình và convention khi phát triển **AutoX.Gara**: branch, code style, thêm tính năng.

---

## 🛠️ 1. Môi trường phát triển

- **IDE**: Visual Studio 2026 hoặc JetBrains Rider (khuyến nghị có workload .NET MAUI và .NET 10).
- **SDK**: .NET 10.
- **Git**: clone repo, làm việc trên branch riêng (xem bên dưới).

---

## 🌿 2. Chi nhánh (Branching)

- **main** (hoặc **master**): nhánh ổn định, sẵn sàng release.
- **develop** (tùy chọn): tích hợp tính năng cho bản tiếp theo.
- **feature/xxx**, **fix/xxx**: nhánh làm tính năng hoặc sửa lỗi, tạo từ `main` hoặc `develop`.

Ví dụ:

- `feature/customer-export` — thêm xuất danh sách khách hàng.
- `fix/login-timeout` — sửa lỗi timeout đăng nhập.

Sau khi xong: tạo **Pull Request** vào `main` (hoặc `develop`), review rồi merge.

---

## 📐 3. Convention code

- **C#**: đặt tên rõ ràng, format nhất quán (theo EditorConfig / style của solution). Tránh bỏ qua nullable khi có thể.
- **Domain**: chỉ entity, value object, logic nghiệp vụ; **không** reference EF Core hay Infrastructure.
- **Application**: class *Ops xử lý use case, gọi Repository/Domain; không biết chi tiết UI hay DB.
- **Infrastructure**: implement Repository, DbContext, Nalix listener/protocol; implement interface từ Domain/Application.
- **Shared**: DTO, request/response, enum; tránh logic nghiệp vụ phức tạp.
- **Frontend**: MVVM — View gắn ViewModel; service gọi Nalix (request/response); tránh logic nghiệp vụ trong View.

Khi thêm project hoặc thay đổi dependency giữa các layer: cập nhật **ARCHITECTURE.md**.

---

## ✨ 4. Thêm tính năng mới (module CRUD)

Gợi ý thứ tự:

1. **Domain**: thêm entity (nếu cần), enum.
2. **Infrastructure**: cấu hình entity trong DbContext, migration; tạo repository (interface + implementation) nếu cần.
3. **Shared**: thêm DTO, QueryRequest, QueryResponse; đăng ký packet trong `AppConfig.Register()`; thêm OpCode trong `OpCommand` nếu lệnh mới.
4. **Application**: thêm class *Ops, đăng ký handler trong Backend `Program.cs` (PacketDispatchChannel).
5. **Backend**: đăng ký Ops và (nếu cần) dependency.
6. **Frontend**: service gọi request/response; ViewModel và View (trang/dialog); đăng ký route/navigation nếu cần.

Chạy lại server + client, kiểm tra CRUD và phân trang (nếu có).

---

## 🐛 5. Debug

- **Server**: đặt breakpoint trong Application (Ops) hoặc Infrastructure; chạy Backend từ IDE (F5). Xem log Nalix.Logging (level Debug khi cần).
- **Client**: chọn Frontend làm startup, target Windows; F5 để debug MAUI. Kiểm tra response từ server (status, payload).
- **Database**: dùng công cụ xem SQLite (DB Browser, Visual Studio) hoặc pgAdmin cho PostgreSQL; so sánh dữ liệu sau khi thao tác.

---

## 📤 6. Commit & Pull Request

- **Commit message**: rõ ràng, có thể dùng prefix: `feat:`, `fix:`, `docs:`, `chore:` (ví dụ: `feat: add customer export to CSV`).
- **PR**: mô tả thay đổi, cách kiểm tra, issue liên quan (nếu có). Đảm bảo build xanh và không phá chức năng hiện có.

Chi tiết đóng góp: [CONTRIBUTING.md](../CONTRIBUTING.md).
