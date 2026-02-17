# Script đổi tên Views từ 11 bước lên 14 bước

## Các file cần đổi tên (theo thứ tự ngược):

```powershell
# Bước 11 (cũ) → Bước 14 (mới): Thanh lý
Rename-Item -Path "Views\Project\Steps\_Step11.cshtml" -NewName "_Step14.cshtml"

# Bước 10 (cũ) → Bước 13 (mới): Nghiệm thu  
Rename-Item -Path "Views\Project\Steps\_Step10.cshtml" -NewName "_Step13.cshtml"

# Bước 9 (cũ) → Bước 10 (mới): Bàn giao (sẽ đổi tên thành "Bàn giao & triển khai thiết bị")
Rename-Item -Path "Views\Project\Steps\_Step9.cshtml" -NewName "_Step10.cshtml"

# Bước 8 (cũ) → Bước 9 (MỚI HOÀN TOÀN): Thử nghiệm Pilot - CẦN TẠO MỚI
# File _Step9.cshtml mới sẽ được tạo

# Bước 7 (cũ) → Bước 8 (mới): Xác nhận tạm ứng
Rename-Item -Path "Views\Project\Steps\_Step7.cshtml" -NewName "_Step8.cshtml"

# Bước 6 (cũ) → Bước 7 (mới): Ký hợp đồng
Rename-Item -Path "Views\Project\Steps\_Step6.cshtml" -NewName "_Step7.cshtml"

# Bước 6 (MỚI): Kiểm tra pháp lý - CẦN TẠO MỚI
# File _Step6.cshtml mới sẽ được tạo

# Bước 11 (MỚI): Đào tạo - CẦN TẠO MỚI
# File _Step11.cshtml mới sẽ được tạo

# Bước 12 (MỚI): Bàn giao hồ sơ kỹ thuật - CẦN TẠO MỚI
# File _Step12.cshtml mới sẽ được tạo
```

## Các file MỚI cần tạo:
1. _Step6.cshtml - Kiểm tra pháp lý
2. _Step9.cshtml - Thử nghiệm Pilot
3. _Step11.cshtml - Đào tạo & chuyển giao vận hành
4. _Step12.cshtml - Bàn giao hồ sơ kỹ thuật

## Các file cần CẬP NHẬT nội dung:
- _Step10.cshtml - Đổi từ "Bàn giao & đào tạo" → "Bàn giao & triển khai thiết bị"
- _Step13.cshtml - Đổi từ "Biên bản nghiệm thu" → "Nghiệm thu"
- _Step14.cshtml - Giữ nguyên "Thanh lý hợp đồng"

## Lưu ý:
- Đổi tên theo thứ tự NGƯỢC (từ 11 → 14, 10 → 13, ...) để tránh conflict
- Sau khi đổi tên xong, tạo các file mới
- Cuối cùng cập nhật nội dung các file đã đổi tên
