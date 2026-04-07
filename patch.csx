var files = new[] {
    @"D:\2025\TPApp\TPApp\TPApp\Areas\Cms\Views\SanPhamCNTB\CreateCongNghe.cshtml",
    @"D:\2025\TPApp\TPApp\TPApp\Areas\Cms\Views\SanPhamCNTB\EditCongNghe.cshtml"
};

var jsToInsert = @"
    // Validation Tab Auto-Scroll
    var firstError = $('.input-validation-error, .field-validation-error').first();
    if (firstError.length > 0) {
        var tabPane = firstError.closest('.tab-pane');
        if (tabPane.length > 0) {
            var tabId = tabPane.attr('id');
            var tabButton = $('button[data-bs-target=""#' + tabId + '""]');
            if (tabButton.length > 0) {
                tabButton.tab('show');
                setTimeout(function() {
                    $('html, body').animate({ scrollTop: tabPane.offset().top - 50 }, 300);
                }, 200);
            }
        }
    }
";

foreach (var path in files) {
    if (!System.IO.File.Exists(path)) {
        Console.WriteLine("Not found: " + path);
        continue;
    }
    var c = System.IO.File.ReadAllText(path);

    // Numbering
    c = c.Replace("6. Bối cảnh & Vấn đề giải quyết", "8. Bối cảnh & Vấn đề giải quyết");
    c = c.Replace("7. Nguyên lý vận hành & Sơ đồ", "9. Nguyên lý vận hành & Sơ đồ quy trình");
    c = c.Replace("8. Thông số kỹ thuật chủ yếu", "10. Thông số kỹ thuật chủ yếu");
    c = c.Replace("9. Ưu điểm nổi bật", "11. Ưu điểm nổi bật");
    c = c.Replace("10. Mức độ sẵn sàng công nghệ TRL", "12. Mức độ sẵn sàng công nghệ TRL");
    c = c.Replace("11. Lĩnh vực ứng dụng", "13. Lĩnh vực ứng dụng");
    c = c.Replace("12. Đối tượng khách hàng mục tiêu", "14. Đối tượng khách hàng mục tiêu");
    c = c.Replace("13. Hiệu quả kinh tế - xã hội", "15. Hiệu quả kinh tế - xã hội");
    c = c.Replace("13. Hình thức chuyển giao", "16. Hình thức chuyển giao");
    c = c.Replace("14. Giá bán dự kiến", "17. Giá bán dự kiến");
    c = c.Replace("15. Các chi phí phát sinh khác", "18. Các chi phí phát sinh khác");
    c = c.Replace("16. Chế độ bảo hành & Hỗ trợ kỹ thuật", "19. Chế độ bảo hành & Hỗ trợ kỹ thuật");
    c = c.Replace("17. Chứng nhận chất lượng", "20. Chứng nhận chất lượng");
    c = c.Replace("18. Dữ liệu đa phương tiện", "21. Dữ liệu đa phương tiện");

    // Replace span validations
    c = c.Replace(@"<select asp-for=""NCUId""", @"<span asp-validation-for=""NCUId"" class=""text-danger small mt-1 d-block""></span>
                            <select asp-for=""NCUId""");
                            
    c = c.Replace(@"<div class=""row g-2"">
                            <div class=""col-auto"">
                                <input type=""radio"" class=""btn-check"" name=""LoaiDeTai""", @"<span asp-validation-for=""LoaiDeTai"" class=""text-danger small mt-1 d-block mb-1""></span>
                        <div class=""row g-2"">
                            <div class=""col-auto"">
                                <input type=""radio"" class=""btn-check"" name=""LoaiDeTai""");
                                
    c = c.Replace(@"<select asp-for=""XuatXuId""", @"<span asp-validation-for=""XuatXuId"" class=""text-danger small mt-1 d-block""></span>
                        <select asp-for=""XuatXuId""");
                        
    c = c.Replace(@"<textarea asp-for=""MoTaNgan""", @"<span asp-validation-for=""MoTaNgan"" class=""text-danger small mt-1 d-block mb-1""></span>
                        <textarea asp-for=""MoTaNgan""");
                        
    c = c.Replace(@"<textarea asp-for=""MoTa""", @"<span asp-validation-for=""MoTa"" class=""text-danger small mt-1 d-block mb-1""></span>
                        <textarea asp-for=""MoTa""");
                        
    c = c.Replace(@"<textarea asp-for=""ThongSo""", @"<span asp-validation-for=""ThongSo"" class=""text-danger small mt-1 d-block mb-1""></span>
                        <textarea asp-for=""ThongSo""");
                        
    c = c.Replace(@"<input type=""hidden"" asp-for=""CategoryId""", @"<span asp-validation-for=""CategoryId"" class=""text-danger small mt-1 d-block mb-1""></span>
                        <input type=""hidden"" asp-for=""CategoryId""");
                        
    c = c.Replace(@"<textarea asp-for=""TargetCustomer""", @"<span asp-validation-for=""TargetCustomer"" class=""text-danger small mt-1 d-block mb-1""></span>
                        <textarea asp-for=""TargetCustomer""");
                        
    c = c.Replace(@"<input type=""hidden"" asp-for=""TransferMethod""", @"<span asp-validation-for=""TransferMethod"" class=""text-danger small mt-1 d-block mb-1""></span>
                        <input type=""hidden"" asp-for=""TransferMethod""");
                        
    c = c.Replace(@"<textarea asp-for=""GiaBanDuKien""", @"<span asp-validation-for=""GiaBanDuKien"" class=""text-danger small mt-1 d-block mb-1""></span>
                        <textarea asp-for=""GiaBanDuKien""");

    c = c.Replace(@"<label class=""form-label fw-semibold"">5. Ảnh đại diện tiêu biểu</label>", @"<label class=""form-label fw-semibold"">5. Ảnh đại diện tiêu biểu</label>
                        <span asp-validation-for=""QuyTrinhHinhAnh"" class=""text-danger small mt-1 d-block mb-1""></span>");

    if (!c.Contains("Validation Tab Auto-Scroll")) {
        c = c.Replace("});\n</script>", jsToInsert + "\n});\n</script>");
    }

    // Numbering fix in Detail.cshtml
    c = c.Replace("6. Bối cảnh", "8. Bối cảnh");

    System.IO.File.WriteAllText(path, c, System.Text.Encoding.UTF8);
}

// Detail.cshtml in CMS
var detailPath = @"D:\2025\TPApp\TPApp\TPApp\Areas\Cms\Views\SanPhamCNTB\Detail.cshtml";
if (System.IO.File.Exists(detailPath)) {
    var c = System.IO.File.ReadAllText(detailPath);
    c = c.Replace("6. Bối cảnh & Vấn đề giải quyết", "8. Bối cảnh & Vấn đề giải quyết");
    c = c.Replace("7. Nguyên lý vận hành & Sơ đồ", "9. Nguyên lý vận hành & Sơ đồ quy trình");
    c = c.Replace("8. Thông số kỹ thuật chủ yếu", "10. Thông số kỹ thuật chủ yếu");
    c = c.Replace("9. Ưu điểm nổi bật", "11. Ưu điểm nổi bật");
    c = c.Replace("10. Mức độ sẵn sàng công nghệ TRL", "12. Mức độ sẵn sàng công nghệ TRL");
    c = c.Replace("11. Lĩnh vực ứng dụng", "13. Lĩnh vực ứng dụng");
    c = c.Replace("12. Đối tượng khách hàng mục tiêu", "14. Đối tượng khách hàng mục tiêu");
    c = c.Replace("13. Hiệu quả kinh tế - xã hội", "15. Hiệu quả kinh tế - xã hội");
    c = c.Replace("13. Hình thức chuyển giao", "16. Hình thức chuyển giao");
    c = c.Replace("14. Giá bán dự kiến", "17. Giá bán dự kiến");
    c = c.Replace("15. Các chi phí phát sinh khác", "18. Các chi phí phát sinh khác");
    c = c.Replace("16. Chế độ bảo hành & Hỗ trợ kỹ thuật", "19. Chế độ bảo hành & Hỗ trợ kỹ thuật");
    c = c.Replace("17. Chứng nhận chất lượng", "20. Chứng nhận chất lượng");
    c = c.Replace("18. Dữ liệu đa phương tiện", "21. Dữ liệu đa phương tiện");
    System.IO.File.WriteAllText(detailPath, c, System.Text.Encoding.UTF8);
}
Console.WriteLine("Done.");
