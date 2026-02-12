-- Check if TechTransferRequest with ID=1 exists
USE [TPApp];
GO

SELECT TOP 10 
    Id,
    HoTen,
    TenCongNghe,
    ProjectId,
    StatusId,
    NgayTao
FROM TechTransferRequests
ORDER BY Id;
GO

-- Check total count
SELECT COUNT(*) as TotalRecords FROM TechTransferRequests;
GO
