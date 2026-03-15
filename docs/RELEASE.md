# 🏷️ Quy trình phát hành (Release Process)

> Đánh version và phát hành bản mới **AutoX.Gara**.

---

## 📌 1. Phiên bản (Versioning)

- Dự án dùng **Semantic Versioning**: `MAJOR.MINOR.PATCH` (ví dụ `1.2.3`).
  - **MAJOR**: Thay đổi không tương thích ngược (API, protocol, database schema phá vỡ).
  - **MINOR**: Tính năng mới tương thích ngược.
  - **PATCH**: Sửa lỗi, cải thiện nhỏ, tài liệu.
- Cập nhật version tại:
  - **Backend**: `AutoX.Gara.Backend.csproj` (Version, AssemblyVersion, FileVersion).
  - **Frontend**: `AutoX.Gara.Frontend.csproj` (ApplicationVersion, ApplicationDisplayVersion).
  - Các project khác (Domain, Application, Infrastructure, Shared) nếu có ghi version trong csproj.
- Đồng bộ một version chung cho cả solution khi phát hành (ví dụ `1.0.0`).

---

## ✅ 2. Chuẩn bị trước khi release

- [ ] Chạy `dotnet build -c Release` — không lỗi.
- [ ] Kiểm tra chức năng chính: đăng nhập, CRUD vài module (customer, part, invoice…).
- [ ] Cập nhật **CHANGELOG.md**: chuyển mục “[Unreleased]” sang version mới (ví dụ `[1.0.0] - 2026-03-15`), thêm link so sánh (compare/tag).
- [ ] Commit với message kiểu: `chore: release v1.0.0`.

---

## 🏷️ 3. Tạo tag và bản release

- **Git tag** (khuyến nghị annotated):
  ```bash
  git tag -a v1.0.0 -m "Release v1.0.0"
  git push origin v1.0.0
  ```
- Trên **GitHub** (hoặc GitLab): tạo **Release** từ tag `v1.0.0`, copy nội dung tương ứng từ CHANGELOG vào mô tả release.
- Đính kèm **artifacts** (tùy chọn):
  - Backend: zip thư mục publish (xem [DEPLOYMENT.md](DEPLOYMENT.md)).
  - Frontend: zip bản publish MAUI Windows (hoặc file cài đặt nếu có).

---

## 📋 4. Sau khi release

- Merge (nếu release từ branch `release/1.0.0`) vào `main`.
- Mở lại mục **[Unreleased]** trong CHANGELOG cho các thay đổi tiếp theo; thêm link so sánh mới (ví dụ `compare/v1.0.0...HEAD`).
- Thông báo (nội bộ hoặc công khai) nếu cần.

---

## 🔧 5. Hotfix (bản vá nhanh)

- Tạo branch từ tag release (ví dụ `v1.0.0`): `git checkout -b hotfix/1.0.1 v1.0.0`.
- Sửa lỗi, tăng **PATCH** (1.0.1), cập nhật CHANGELOG.
- Tag `v1.0.1`, tạo release, merge vào `main` (và `main` merge vào các branch phát triển nếu có).

---

Xem thêm: [CHANGELOG.md](../CHANGELOG.md), [DEPLOYMENT.md](DEPLOYMENT.md).
