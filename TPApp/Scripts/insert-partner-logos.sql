-- ============================================================
-- INSERT ImagesAdver: Mạng lưới đối tác (Subject 2, 3, 5)
-- SiteId = 1, LanguageID = 1, StatusID = 3 (Xuất bản)
-- Domain = 'techport.vn'
-- NOTE: Run ProcessLogos first to generate sanitized files & thumbnails
--       After fixing Đ→D bug, re-run ProcessLogos to fix Dovetec-Dong-Thap
-- ============================================================

-- ══════════════════════════════════════════════════
-- Subject = 2: Hệ thống Sàn giao dịch công nghệ
-- ══════════════════════════════════════════════════
INSERT INTO [ImagesAdver] ([Title], [SRC], [URL], [Subject], [StatusID], [Sort], [LanguageID], [Domain], [SiteId], [Created], [Creator])
VALUES
(N'SGDCN TP.HCM',
 N'/uploads/logo/Logo-SGDCN/San-giao-dich-cong-nghe-TP-HCM.jpg',
 N'https://techport.vn/index.html',
 2, 3, 1, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'SGDCN Gia Lai',
 N'/uploads/logo/Logo-SGDCN/San-giao-dich-cong-nghe-va-thiet-bi-Gia-Lai.png',
 N'https://sancongnghegialai.vn/index.html',
 2, 3, 2, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'SGDCN Tây Ninh',
 N'/uploads/logo/Logo-SGDCN/San-giao-dich-cong-nghe-va-thiet-bi-Tay-Ninh.png',
 N'https://sgdcn.tayninh.gov.vn/index.html',
 2, 3, 3, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'SGDCN Đồng Tháp',
 N'/uploads/logo/Logo-SGDCN/Dovetec-Dong-Thap.png',
 N'https://dovetec.techinnovation.vn/index.html',
 2, 3, 4, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'SGDCN Quốc gia',
 N'/uploads/logo/Logo-SGDCN/San-giao-dich-Khoa-hoc-cong-nghe-quoc-gia.png',
 NULL,
 2, 3, 5, 1, N'techport.vn', 1, GETDATE(), N'admin');


-- ══════════════════════════════════════════════════
-- Subject = 3: Đơn vị tư vấn
-- ══════════════════════════════════════════════════
INSERT INTO [ImagesAdver] ([Title], [SRC], [URL], [Subject], [StatusID], [Sort], [LanguageID], [Domain], [SiteId], [Created], [Creator])
VALUES
(N'Viettel TP.HCM',
 N'/uploads/logo/Logo-don-vi-tu-van/Viettel.png',
 N'https://www.viettel.com.vn',
 3, 3, 1, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'VNPT TP.HCM',
 N'/uploads/logo/Logo-don-vi-tu-van/VNPT.jpg',
 N'https://www.vnpt.vn',
 3, 3, 2, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'VN PAY',
 N'/uploads/logo/Logo-don-vi-tu-van/vn-pay.png',
 N'https://www.vnpay.vn',
 3, 3, 3, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'Hiệp hội DN KH&CN',
 N'/uploads/logo/Logo-don-vi-tu-van/Hiep-hoi-doanh-nghiep-khcn.jpg',
 N'https://www.vsta.org.vn',
 3, 3, 4, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'ĐH CN Kỹ Thuật',
 N'/uploads/logo/Logo-don-vi-tu-van/Truong-dh-cong-nghe-ky-thuat.jpg',
 N'https://www.hutech.edu.vn',
 3, 3, 5, 1, N'techport.vn', 1, GETDATE(), N'admin');


-- ══════════════════════════════════════════════════
-- Subject = 5: Đối tác
-- ══════════════════════════════════════════════════
INSERT INTO [ImagesAdver] ([Title], [SRC], [URL], [Subject], [StatusID], [Sort], [LanguageID], [Domain], [SiteId], [Created], [Creator])
VALUES
(N'Becamex',
 N'/uploads/logo/Logo-doi-tac/Becamex.jpg',
 N'https://www.becamex.com.vn',
 5, 3, 1, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'ĐH Bách Khoa',
 N'/uploads/logo/Logo-doi-tac/Bach-Khoa.jpg',
 N'https://www.hcmut.edu.vn',
 5, 3, 2, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'ĐH Công Thương',
 N'/uploads/logo/Logo-doi-tac/Truong-dh-cong-thuong.jpeg',
 N'https://www.huit.edu.vn',
 5, 3, 3, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'TT Công nghệ SH',
 N'/uploads/logo/Logo-doi-tac/TT-CNSH.png',
 N'https://www.hcmbiotech.com.vn',
 5, 3, 4, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'TT NC ĐH Nông Lâm',
 N'/uploads/logo/Logo-doi-tac/TT-Nghien-cuu-chuyen-giao-khcn-Truong-DH-Nong-Lam.jpg',
 N'https://www.hcmuaf.edu.vn',
 5, 3, 5, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'TT ĐH Quốc tế',
 N'/uploads/logo/Logo-doi-tac/TT-DMST-CGCN-Truong-dh-quoc-te.png',
 N'https://www.hcmiu.edu.vn',
 5, 3, 6, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'SHTP Labs',
 N'/uploads/logo/Logo-doi-tac/TT-nghien-cuu-trien-khai-khu-cong-nghe-cao.jpg',
 N'https://www.shtplabs.org',
 5, 3, 7, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'TT NC NN CNC',
 N'/uploads/logo/Logo-doi-tac/TT-nghien-cuu-phat-trien-nong-nghiep-cnc.png',
 N'https://www.hcmbiotech.com.vn',
 5, 3, 8, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'CSID',
 N'/uploads/logo/Logo-doi-tac/TT-phat-trien-dich-vu-khai-thac-ha-tang-khcn.png',
 N'https://www.pvcodiensauthuhoach.com',
 5, 3, 9, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'Siaep',
 N'/uploads/logo/Logo-doi-tac/Phan-vien-co-dien.png',
 N'https://www.pvcodiensauthuhoach.com',
 5, 3, 10, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'Hội Sáng chế VN',
 N'/uploads/logo/Logo-doi-tac/Hoi-sang-che-vn.jpg',
 N'https://www.hoisangche.org.vn',
 5, 3, 11, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'HCA TP.HCM',
 N'/uploads/logo/Logo-doi-tac/Hoi-tin-hoc-tphcm.jpg',
 N'https://www.hca.org.vn',
 5, 3, 12, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'HAMEE',
 N'/uploads/logo/Logo-doi-tac/Hoi-co-khi.jpg',
 N'https://www.hamee.vn',
 5, 3, 13, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'VIETFAIR',
 N'/uploads/logo/Logo-doi-tac/Vietfair.jpg',
 N'https://www.vietfair.vn',
 5, 3, 14, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'Analytica Vietnam',
 N'/uploads/logo/Logo-doi-tac/analytica.jpg',
 N'https://www.analyticavietnam.com',
 5, 3, 15, 1, N'techport.vn', 1, GETDATE(), N'admin'),

(N'SnE Company',
 N'/uploads/logo/Logo-doi-tac/SnE.png',
 NULL,
 5, 3, 16, 1, N'techport.vn', 1, GETDATE(), N'admin');

-- Verify
SELECT Subject, COUNT(*) AS [Count] 
FROM ImagesAdver 
WHERE Subject IN (2, 3, 5) AND StatusID = 3
GROUP BY Subject;
