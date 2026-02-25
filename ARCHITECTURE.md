# 🧱 Tổng quan kiến trúc đề xuất (summary)

- 🖥️ **UI**: Avalonia (.NET 8) — ứng dụng client (máy trạm).  
- 🛠️ **Server**: .NET 10 console/worker service (hoặc .NET 10 minimal host) — vì Nalix hiện bạn viết ở .NET 10.  
- 📡 **Communication**: TCP messages qua Nalix (request/response tuỳ chỉnh + events để push cập nhật thời gian thực).  
- 🗄️ **Database**: Khuyến nghị dùng **SQL Server (Express)** trên server làm DB trung tâm (concurrency, ACID, scale).  
  - Tuỳ chọn: **PostgreSQL** nếu muốn chạy server trên Linux.  
  - Client local cache (tùy chọn) → **SQLite** hoặc **LiteDB** (nếu sau này cần offline).  
- 🧬 **ORM**: EF Core (chạy phía server) với provider SQL Server.  
- 🔐 **Authentication/Authorization**: RBAC phía server (role–permissions). Client chỉ gửi thông tin đăng nhập/token; server kiểm tra và thực thi quyền.  
- 🌐 **Localization**: dùng lớp **MultiLocalizer** phía client (VN / EN) như bạn đã đề cập.  
- 🔄 **Auto‑update**: tuỳ chọn.  
  - Thường sẽ dùng **HTTP/HTTPS**.  
  - Nếu bạn muốn **chỉ dùng TCP**, có thể truyền file cập nhật qua Nalix nhưng cần hỗ trợ file-transfer trong Nalix → phức tạp hơn.  
  - 👉 Nên cho phép dùng HTTP/HTTPS để update (dễ, chuẩn, an toàn).  
- 🖨️ **Printing**: tạm hoãn, sẽ bổ sung sau.  

---

## 🗃️ Tại sao chọn SQL Server (server) và SQLite (client optional)

### 🧩 SQL Server (Express)

- ✅ **Ưu điểm**:
  - Concurrency mạnh cho nhiều client đồng thời.
  - Có công cụ quản trị **SSMS** (SQL Server Management Studio).
  - Bảo đảm giao dịch (**transactional safety**, ACID).
  - Dễ mở rộng trên Windows Server.
  - EF Core hỗ trợ tốt, ổn định, nhiều tài liệu.  
- ⚠️ **Nhược điểm**:
  - Nặng hơn SQLite.
  - Cần cài đặt và quản lý trên máy chủ.

### 💾 SQLite (client-cache)

- ✅ **Ưu điểm**:
  - Nhẹ, chỉ là 1 file `.db`.
  - Phù hợp làm **cache offline** cục bộ cho từng client.
- ⚠️ **Nhược điểm**:
  - Concurrency hạn chế nếu nhiều process cùng ghi.  

👉 Với kiến trúc **1 server – nhiều client**, khuyến nghị:

- **SQL Server** trên server để lưu **master data** (dữ liệu chuẩn).  
- Nếu sau này cần **offline-first**, bổ sung **SQLite cache** cục bộ cho từng client, có cơ chế sync riêng.

---

## 📚 Mô hình dữ liệu tổng quan (core entities)

- 👤 **User**  
  `Id, Username, PasswordHash, Salt, FullName, RoleId, IsActive`
- 🧩 **Role**  
  `Id, Name, Permissions`
- 🛡️ **Permission**  
  Enum / danh sách quyền (ví dụ: ViewInventory, EditInventory, ManageUsers, …)
- 👔 **Employee**  
  `Id, UserId, Position, Contact`
- 👥 **Customer / Partner**
- 🚗 **Vehicle**  
  `Id, CustomerId, LicenceNumber, Model, ...`
- 📦 **InventoryItem**  
  `Id, SKU, Name, QtyOnHand, MinQty, Location, UnitCost`
- 🔁 **StockTransaction**  
  `Id, ItemId, Qty, Type: IN/OUT/ADJUST, Reference, CreatedBy, Timestamp`
- 🧾 **WorkOrder / RepairJob**  
  `Id, VehicleId, AssignedTo, Status, PartsUsed, LaborHours, TotalCost`
- 🕒 **Timesheet** (điểm danh, chấm công – để phát triển sau)  
- 📝 **AuditLog** (ghi lại hành động quan trọng)  

➡️ **Server** sẽ mở các command qua message Nalix và trả về **DTO** cho client.

---

## 🔁 Flow giao tiếp (đơn giản hoá)

1. 🔐 **Đăng nhập**
   - Client → Server: `AuthRequest` (username/password)  
   - Server:
     - Kiểm tra tài khoản + mật khẩu (hash + salt).
     - Nếu hợp lệ → trả về `AuthResponse` chứa `SessionToken` (hoặc JWT).  

2. 📤 **Thực thi lệnh (command)**  
   - Client → Server: các command (ví dụ: `CreateWorkOrder`, `GetInventoryList`) **luôn** kèm `SessionToken`.  
   - Server:
     - Xác thực token.
     - Kiểm tra RBAC (quyền của user).
     - Thực thi logic + truy cập DB.
     - Trả về DTO hoặc error code.

3. 📥 **Push event (realtime)**  
   - Server → Client: đẩy các sự kiện như:
     - `InventoryChanged`
     - `WorkOrderCreated`
     - `WorkOrderUpdated`  
   - Chỉ push tới các client đã **subscribe** kênh phù hợp (vd: theo chi nhánh, vai trò, …).

4. ⚖️ **Xử lý xung đột (conflict)**  
   - Server là **“single source of truth”** (nguồn dữ liệu chuẩn).  
   - Client luôn sync thời gian thực.  
   - Nếu sau này có **offline mode**, sẽ bổ sung cơ chế sync/merge (version, last-updated, …) riêng.

---

## 🗓️ Kế hoạch sprint & tasks (4–6 tuần)

> Chia thành **4 sprint** (mỗi sprint ~1–2 tuần, linh hoạt tuỳ thời gian học và làm).

---

### 🏁 Sprint 0 — Chuẩn bị (2–3 ngày)

- 🧰 Cài đặt / chuẩn bị tooling:
  - .NET 8 SDK
  - .NET 10 SDK (server)
  - Avalonia templates
  - SQL Server Express (hoặc Docker image)
  - EF Core tools
- 📁 Khởi tạo Git repo, thiết lập `.gitignore`.  
- 🧱 Tạo skeleton solution:
  - Project Server
  - Project Client
  - Project Shared/Domain  
- 🎯 **Deliverable**: solution skeleton build được, chạy “Hello World” cho server + client.

---

### ⚙️ Sprint 1 — Core Server + Models + Auth + Basic Inventory API (≈ 1 tuần)

- 🗄️ Thiết kế model EF Core & `DbContext`, tạo migration đầu tiên.  
- 👤 Xây dựng User/Role/Permission + seeding **admin user** mặc định.  
- 📡 Thiết kế skeleton message handler cho Nalix và luồng Auth cơ bản:
  - `AuthRequest` / `AuthResponse`
- 📦 Cài đặt các command CRUD cho `InventoryItem` (qua Nalix):
  - `CreateInventoryItem`
  - `UpdateInventoryItem`
  - `DeleteInventoryItem`
  - `GetInventoryList`
- 📜 Thiết lập logging (Serilog hoặc Nalix.Logging) + cấu hình cơ bản:
  - Log file rolling, log level theo môi trường.  
- 🎯 **Deliverable**:  
  - Server chạy local.  
  - Client test tool (hoặc console) có thể gửi message Inventory CRUD và nhận response.

---

### 💻 Sprint 2 — Client Avalonia skeleton + Login + Inventory UI (≈ 1 tuần)

- 🧱 Tạo skeleton ứng dụng **Avalonia**:
  - Shell chính (MainWindow, Navigation).
  - Placeholder **MultiLocalizer** (VN/EN).  
- 🔑 Màn hình **Login**:
  - Gửi `AuthRequest` qua Nalix.
  - Lưu `SessionToken` nếu đăng nhập thành công.  
- 📋 Màn hình **Inventory**:
  - Grid/list hiển thị danh sách `InventoryItem`.
  - Dialog **Create/Edit** item.
- 💾 Local caching (tùy chọn, chỉ đọc):
  - Lưu cache inventory để load nhanh hơn / giữ data khi server tạm mất kết nối.  
- 🎯 **Deliverable**:
  - Client có thể login.
  - Client CRUD inventory qua server bằng giao diện Avalonia.

---

### 🧑‍💼 Sprint 3 — Roles & Users Management + Stock Transactions + WorkOrder CRUD (≈ 1 tuần)

- 🛡️ **Server**:
  - RBAC enforcement “middleware” / tầng check quyền cho **mọi command**.
  - Command quản lý user/role:
    - Tạo user, đổi role, khoá/mở khoá tài khoản.  
- 📦 **StockTransaction**:
  - Model + logic business:
    - Cập nhật tồn kho khi nhập/xuất/điều chỉnh.
    - Kiểm tra không cho tồn âm (trừ khi cho phép).
    - Cảnh báo khi `QtyOnHand < MinQty`.  
- 🧑‍💻 **Client**:
  - Màn hình quản lý user/role (Admin).
  - Màn hình WorkOrder:
    - Danh sách WorkOrder.
    - Tạo/sửa WorkOrder cơ bản (chọn vehicle, parts, laborHours, status…).  
- 🎯 **Deliverable**:
  - Flow inventory đầy đủ với **StockTransaction**.
  - RBAC hoạt động (user thường vs admin).
  - WorkOrder CRUD hoạt động ở mức cơ bản.

---

### 📊 Sprint 4 — Khung báo cáo + Audit + Packaging + Nghiên cứu Auto‑update (≈ 1 tuần)

- 📝 **AuditLog**:
  - Ghi lại các hành động quan trọng:
    - Login/Logout.
    - CRUD Inventory, StockTransaction, WorkOrder.
    - Thay đổi quyền, user.  
- 📈 **Báo cáo đơn giản**:
  - Endpoint/report tóm tắt:
    - Tổng tồn kho theo Item.
    - Doanh thu đơn giản theo WorkOrder (ngày/tháng).  
- ⚙️ **CI & Packaging**:
  - Thêm CI (GitHub Actions) pipeline:
    - `dotnet build`
    - `dotnet test`
    - Tạo artifacts publish cho server và client.
  - Nghiên cứu packaging:
    - MSIX / Squirrel / installer cơ bản.
- 📦 **Auto-update**:
  - Thiết kế/prototype cơ chế check update:
    - Kiểm tra manifest update qua HTTPS (gợi ý).
    - Hoặc placeholder service chờ tích hợp Nalix file-transfer sau.  
- 🎯 **Deliverable**:
  - MVP sẵn sàng cho test nội bộ.
  - Có hướng dẫn cài đặt (runbook) & kế hoạch/prototype auto-update.

> ⏱️ Tổng estimate: **~4 tuần** cho MVP core (nếu tập trung), có thể **6 tuần** nếu vừa làm vừa học chậm hơn.  
> Các module nâng cao (báo cáo chi tiết, chấm công, …) để phase sau.

---

## 🛡️ Checklist bảo mật & hiệu năng (bắt buộc)

- 🔐 **Giao tiếp an toàn**
  - Luôn dùng **TLS** cho mọi giao tiếp từ xa.  
  - Nếu Nalix chạy TCP thuần:
    - Bọc thêm lớp TLS (SslStream), hoặc  
    - Đảm bảo Nalix đã hỗ trợ kênh mã hoá.  
  - **Không** gửi mật khẩu dạng plain-text.  

- 🔑 **Password**
  - Lưu mật khẩu dưới dạng **salted hash**:
    - Ưu tiên **PBKDF2** hoặc **Argon2**.  

- 🧩 **RBAC**
  - RBAC được thực thi **phía server** cho **tất cả** command (không tin client).  

- 🧹 **Input validation**
  - Kiểm tra dữ liệu đầu vào trên server:
    - Độ dài, kiểu, pattern, range.
  - Không tin dữ liệu từ client, kể cả UI do mình viết.  

- 🧯 **Chống injection**
  - Dùng truy vấn tham số / EF Core (không ghép string SQL).  

- 🧾 **Logging**
  - Có thời gian lưu trữ hợp lý (log rotation).
  - Không log dữ liệu nhạy cảm (mật khẩu, token, số thẻ, …).  

- 🚫 **Brute-force**
  - Giới hạn số lần đăng nhập sai (rate limiting, lockout tạm thời).  

- 💽 **Backup**
  - Sao lưu DB định kỳ (full/differential theo tuần/ngày).
  - Kiểm tra khôi phục (restore test) định kỳ.  

- 📑 **Pagination**
  - Dùng phân trang cho danh sách lớn (inventory, workorders, logs).
  - Tránh load toàn bộ dữ liệu về client một lần.

---

## 🔄 Chiến lược sync & xử lý xung đột (khi thêm offline sau này)

- 🧭 **Server authoritative**
  - Server là nguồn dữ liệu chuẩn.
  - Chỉ chấp nhận update từ client nếu version hiện tại khớp với version trên server.  
  - Nếu lệch → trả về lỗi “**conflict**” → client tự xử lý hoặc tải lại dữ liệu.  

- ♻️ **Idempotent commands**
  - Dùng `requestId` do client sinh để:
    - Nếu gửi lại request (retry) → server không xử lý trùng lặp.  

- ⚖️ **Nghiệp vụ kho (stock)**
  - Tất cả logic cập nhật tồn kho phải được gói trong **transaction** ở DB:
    - Đảm bảo không bị mất dữ liệu khi nhiều thao tác đồng thời.

---

## 🔁 Ghi chú về auto-update

- 📥 Auto-update cho desktop thường dùng **HTTP(S)** để tải bản cập nhật:
  - Squirrel
  - MSIX
  - Hoặc giải pháp custom.  
- Nếu chính sách không cho dùng HTTP:
  - Có thể truyền file cập nhật qua **Nalix** (TCP file transfer).
  - Nhưng sẽ **phức tạp hơn** (phân mảnh file, resume, checksum, …).  
- ✅ Khuyến nghị:
  - Cho phép dùng **HTTPS** để phân phối update (chuẩn, an toàn, ít việc).  
  - Thiết kế 1 `UpdateService`:
    - Kiểm tra manifest update (HTTP).
    - Tải và apply update.
    - Sau này nếu cần, đổi kênh tải thành TCP/Nalix mà vẫn reuse logic.

---

## 💾 Gợi ý Backup & Restore (ban đầu)

- 🗄️ **Server**
  - Dùng backup full/differential của SQL Server:
    - Full backup định kỳ (vd: tuần).
    - Differential / log backup hàng ngày hoặc vài giờ.  
  - Có thể dùng SQL Agent job hoặc script chạy theo lịch (Task Scheduler).  

- 📤 **Export/Restore UI**
  - Cung cấp giao diện admin:
    - Export dữ liệu quan trọng dạng JSON/CSV (khách hàng, vehicle, inventory, workorders,…).
    - Tối thiểu có **Export** để cứu data khi cần.  

- 🧾 **AuditLog**
  - Lưu trữ tách biệt (bảng riêng).
  - Có cơ chế xoá/rotate (vd: chỉ giữ 6–12 tháng).

---

## 🌏 Localization

- 🧩 Client dùng service `MultiLocalizer`:
  - Dựa trên file resource (`.resx` hoặc JSON).  
- 🈶 Giao diện hỗ trợ tối thiểu:
  - **Tiếng Việt**  
  - **Tiếng Anh**  
- 🔄 Có thể chuyển đổi ngôn ngữ **runtime** (không cần restart app nếu thiết kế tốt).

---

## 🧪 Testing & CI

- 🧠 **Unit test (Domain)**
  - Kiểm tra:
    - Validation của `StockTransaction`.
    - Tính toán tồn kho.
    - Tính tổng tiền `WorkOrder` (parts + labor).  

- 🔗 **Integration test (Server + Nalix)**
  - Giả lập message Nalix:
    - Gửi `AuthRequest`, `CreateInventoryItem`, …  
    - Kiểm tra server xử lý và trả về đúng.  

- ⚙️ **CI pipeline**
  - Sử dụng GitHub Actions (hoặc tool tương đương) để:
    - `dotnet build` solution.
    - `dotnet test` toàn bộ test.
    - Sinh **artifacts**:
      - Bản publish cho **server**.
      - Bản publish/cài đặt cho **client**.

---
