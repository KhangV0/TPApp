# Hướng Dẫn Chạy SQL Script Populate Data

## 📁 File Location
```
d:\2026\Techport\TPApp\Database\Migrations\MANUAL_Populate_200Products.sql
```

## 🚀 Cách Chạy

### Option 1: SQL Server Management Studio (SSMS) - RECOMMENDED
1. Mở SSMS
2. Connect to `localhost` với user `sa`
3. File → Open → chọn file `MANUAL_Populate_200Products.sql`
4. Nhấn **F5** hoặc click **Execute**
5. Đợi ~30-60 giây để script complete

### Option 2: Command Line (nếu SSMS không available)
```powershell
cd d:\2026\Techport\TPApp
sqlcmd -S localhost -d TechMartNew -U sa -P 111111 -i Database\Migrations\MANUAL_Populate_200Products.sql
```

## ✅ Script Sẽ Làm Gì

1. **Create Function** `fnRemoveVietnameseAccents` (nếu chưa có)
   - Removes Vietnamese accents for search
   - Example: "máy dò" → "may do"

2. **Insert 200 Products** vào `SearchIndexContents`
   - Lấy từ `SanPhamCNTB` table
   - Chỉ products với `StatusId = 3` và `LanguageId = 1`
   - Skip products đã tồn tại (check by `RefId`)

3. **Verify Data**
   - Show total products inserted
   - Show sample 5 products

4. **Test FullText Search**
   - Test 1: "máy dò" (with accents)
   - Test 2: "thiết bị" (with accents)
   - Test 3: "may do" (without accents)

## 📊 Expected Output

```
Function fnRemoveVietnameseAccents already exists.

Inserting 200 products...
200 products inserted.

==============================================
Verification
==============================================
TotalProducts
-------------
200

Sample products:
Id    Title                          TypeName    RefId
---   ----------------------------   ---------   -----
1     Máy dò kim loại XYZ           Product     123
2     Thiết bị đo lường ABC         Product     124
...

==============================================
Testing FullText Search
==============================================

Test 1: Search for "máy dò"
(Should return products with "máy dò" in title)

Test 2: Search for "thiết bị"
(Should return products with "thiết bị" in title)

Test 3: Search for "may do" (no accents)
(Should return products with "máy dò" using RemovedUnicode column)

==============================================
DONE! SearchIndexContents populated successfully.
==============================================
```

## 🔍 Sau Khi Chạy Script

### 1. Verify Data
```sql
-- Check total records
SELECT COUNT(*) FROM SearchIndexContents;

-- Check by TypeName
SELECT TypeName, COUNT(*) 
FROM SearchIndexContents 
GROUP BY TypeName;

-- Sample data
SELECT TOP 10 * FROM SearchIndexContents;
```

### 2. Test Search trong App
1. Build lại app: `dotnet build`
2. Run app: `dotnet run`
3. Navigate to: `http://localhost:5000/search?q=máy dò`
4. Should see search results!

### 3. Test Different Keywords
- "máy dò kim loại"
- "thiết bị đo lường"
- "may do" (no accents)
- "thiet bi" (no accents)

## ⚠️ Troubleshooting

### Error: "Cannot insert NULL into column 'Id'"
**Solution:** Table có constraint issue. Check:
```sql
sp_help SearchIndexContents
```

### Error: "Invalid object name 'fnRemoveVietnameseAccents'"
**Solution:** Function chưa được tạo. Script sẽ tự tạo, nhưng nếu fail:
```sql
-- Check if function exists
SELECT * FROM sys.objects WHERE name = 'fnRemoveVietnameseAccents';
```

### No Results in Search
**Possible causes:**
1. FullText index chưa populate → Wait 1-2 minutes
2. FullText service not running → Restart SQL Server
3. Data chưa được insert → Check `SELECT COUNT(*) FROM SearchIndexContents`

### Slow Performance
**Solution:** FullText index đang rebuild. Đợi 2-5 phút sau khi insert data.

## 📝 Next Steps After Success

1. ✅ Data populated (200 products)
2. ✅ FullText search working
3. 🔄 Build và run app
4. 🔄 Test search UI
5. 🔄 Populate thêm data (Suppliers, News, etc.) nếu cần

## 🎯 Full Data Population (Optional)

Nếu muốn populate ALL data (not just 200 products), run:
```
Database\Migrations\004_PopulateSearchIndexContents.sql
```

**Warning:** Script này sẽ mất 5-10 phút để chạy vì populate từ 11 data sources.

---

**Created:** 2026-02-16  
**Purpose:** Quick data population for search system testing
