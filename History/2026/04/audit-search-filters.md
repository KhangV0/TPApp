# Kiểm tra bộ lọc tìm kiếm CMS — 2026-04-11

> **Chú thích ký hiệu:**
> - ✅ Có sẵn và hoạt động đúng
> - ❌ Còn thiếu — cần thêm
> - ⚠️ Có trường trong DB nhưng chưa đưa vào filter UI

---

## 1. Công nghệ (`/cms/SanPhamCNTB/CongNghe`)

**Controller:** `SanPhamCNTBController.CongNghe` → `ListByType(TypeCongNghe)`

| Bộ lọc yêu cầu | Tên field | Trạng thái |
|----------------|-----------|-----------|
| Tên / Mã | `keyword` | ✅ |
| Trạng thái xuất bản | `statusId` | ✅ |
| Nhà cung ứng | `ncuId` | ✅ |
| Xuất xứ | `xuatXuId` | ✅ |
| Sàn GDCN | `siteId` | ✅ |
| Ngày tạo từ/đến | `createdFrom`, `createdTo` | ✅ |
| Lĩnh vực | `linhVucId` | ❌ **Thiếu** |
| Người tạo | `creator` | ❌ **Thiếu** |
| TRL (mức độ sẵn sàng công nghệ) | `mucDoId` | ❌ **Thiếu** |

---

## 2. Thiết bị (`/cms/SanPhamCNTB/ThietBi`)

**Controller:** `SanPhamCNTBController.ThietBi` → `ListByType(TypeThietBi)` _(cùng form với Công nghệ)_

| Bộ lọc yêu cầu | Tên field | Trạng thái |
|----------------|-----------|-----------|
| Tên / Mã | `keyword` | ✅ |
| Trạng thái xuất bản | `statusId` | ✅ |
| Nhà cung ứng | `ncuId` | ✅ |
| Xuất xứ | `xuatXuId` | ✅ |
| Sàn GDCN | `siteId` | ✅ |
| Ngày tạo từ/đến | `createdFrom`, `createdTo` | ✅ |
| Lĩnh vực | `linhVucId` | ❌ **Thiếu** |
| Người tạo | `creator` | ❌ **Thiếu** |

---

## 3. Tài sản trí tuệ (`/cms/SanPhamCNTB/SanPhamTriTue`)

**Controller:** `SanPhamCNTBController.SanPhamTriTue` → `ListByType(TypeSanPhamTriTue)` _(cùng form)_

| Bộ lọc yêu cầu | Tên field | Trạng thái |
|----------------|-----------|-----------|
| Tên / Mã | `keyword` | ✅ |
| Trạng thái xuất bản | `statusId` | ✅ |
| Chủ sở hữu (Nhà cung ứng) | `ncuId` | ✅ (dùng chung ncuId) |
| Sàn GDCN | `siteId` | ✅ |
| Ngày tạo từ/đến | `createdFrom`, `createdTo` | ✅ |
| Lĩnh vực | `linhVucId` | ❌ **Thiếu** |
| Người tạo | `creator` | ❌ **Thiếu** |
| Phân loại hồ sơ | `categoryId` | ❌ **Thiếu** |

---

## 4. Nhà cung ứng (`/cms/NhaCungUngAdmin`)

**Controller:** `NhaCungUngAdminController.Index`

| Bộ lọc yêu cầu | Tên field | Trạng thái |
|----------------|-----------|-----------|
| Tên | `keyword` | ✅ |
| Trạng thái xuất bản | `statusId` | ✅ |
| Ngày tạo từ/đến | `createdFrom`, `createdTo` | ✅ |
| Kích hoạt | `isActivated` | ✅ (bonus) |
| Lĩnh vực | `linhVucId` | ❌ **Thiếu** |
| Người tạo | `createdBy` | ❌ **Thiếu** |
| Sàn GDCN | `siteId` | ❌ **Thiếu** |
| Dịch vụ KH&CN | `dichVuId` | ❌ **Thiếu** |

---

## 5. Nhà tư vấn (`/cms/NhaTuVanAdmin`)

**Controller:** `NhaTuVanAdminController.Index`

| Bộ lọc yêu cầu | Tên field | Trạng thái |
|----------------|-----------|-----------|
| Tên | `keyword` | ✅ |
| Trạng thái xuất bản | `statusId` | ✅ |
| Ngày tạo từ/đến | `createdFrom`, `createdTo` | ✅ |
| Kích hoạt | `isActivated` | ✅ (bonus) |
| Lĩnh vực | `linhVucId` | ❌ **Thiếu** |
| Người tạo | `createdBy` | ❌ **Thiếu** |
| Sàn GDCN | `siteId` | ❌ **Thiếu** |
| Dịch vụ tư vấn | `dichVuId` | ❌ **Thiếu** |

---

## 6. Tin bài (`/cms/Posts`)

**Controller:** `PostsController.Index`

| Bộ lọc yêu cầu | Tên field | Trạng thái |
|----------------|-----------|-----------|
| Tên / Từ khóa | `keyword` | ✅ |
| Trạng thái xuất bản | `statusId` | ✅ |
| Danh mục tin (Menu) | `menuId` | ✅ |
| Sàn GDCN | `siteId` | ✅ |
| Ngày đăng từ/đến | `dateFrom`, `dateTo` | ✅ |
| Bài Hot / Mới | `isHot`, `isNew` | ✅ (bonus) |
| Người đăng (tác giả) | `author` | ❌ **Thiếu** |

---

## Tổng kết — Danh sách cần bổ sung

| Module | Cần thêm |
|--------|----------|
| **Công nghệ** | Lĩnh vực, Người tạo, TRL (MucDoId) |
| **Thiết bị** | Lĩnh vực, Người tạo |
| **Tài sản trí tuệ** | Lĩnh vực, Người tạo, Phân loại hồ sơ (CategoryId) |
| **Nhà cung ứng** | Lĩnh vực, Người tạo, Sàn GDCN, Dịch vụ KH&CN |
| **Nhà tư vấn** | Lĩnh vực, Người tạo, Sàn GDCN, Dịch vụ tư vấn |
| **Tin bài** | Người đăng (Author) |

> **Ghi chú kỹ thuật:**
> - "Lĩnh vực" trong `SanPhamCNTB` có thể map với `CategoryId` hoặc một field riêng — cần xác nhận tên cột thực tế trong DB.
> - "Người tạo" là `Creator` (string) trong `SanPhamCNTB`, `CreatedBy` (string) trong `NhaCungUng`/`NhaTuVan` — có thể lọc bằng keyword hoặc dropdown.
> - "TRL" trong Công nghệ là field `MucDoId` (int FK).
> - "Dịch vụ KH&CN" / "Dịch vụ tư vấn" là multi-value string field, cần xem cách lưu để filter phù hợp.
