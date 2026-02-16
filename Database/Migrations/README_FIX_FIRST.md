# ⚠️ IMPORTANT - Run This First!

## Vấn đề
Column `Id` trong table `SearchIndexContents` **KHÔNG phải IDENTITY** (auto-increment), nên INSERT bị lỗi:
```
Cannot insert the value NULL into column 'Id'
```

## Giải pháp

### Step 1: Fix Table Structure
Chạy script này TRƯỚC:
```
Database\Migrations\FIX_SearchIndexContents_Identity.sql
```

**Script sẽ làm gì:**
1. ✅ Create new table với `Id BIGINT IDENTITY(1,1)`
2. ✅ Copy existing data (nếu có)
3. ✅ Drop old table
4. ✅ Rename new table
5. ✅ Recreate FullText index

**Thời gian:** ~30 giây

### Step 2: Populate Data
Sau khi fix xong, chạy:
```
Database\Migrations\SIMPLE_Populate_200Products.sql
```

## Cách chạy (SSMS)

1. **Open:** `FIX_SearchIndexContents_Identity.sql`
2. **Execute** (F5)
3. **Wait** ~30 seconds
4. **Verify:** Should see "DONE! Table fixed with IDENTITY column"
5. **Then run:** `SIMPLE_Populate_200Products.sql`

## Verification

Sau khi chạy FIX script, verify:
```sql
-- Check if Id is IDENTITY
SELECT 
    COLUMN_NAME,
    COLUMNPROPERTY(OBJECT_ID('SearchIndexContents'), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SearchIndexContents' AND COLUMN_NAME = 'Id';

-- Should return: IsIdentity = 1
```

## ⚠️ Warning

Script này sẽ:
- ❌ Drop và recreate table
- ❌ Drop và recreate FullText index
- ✅ Preserve existing data (nếu có)

Nếu table đang có data quan trọng, backup trước!

---

**Run Order:**
1. `FIX_SearchIndexContents_Identity.sql` ← **RUN THIS FIRST**
2. `SIMPLE_Populate_200Products.sql` ← Run after fix
