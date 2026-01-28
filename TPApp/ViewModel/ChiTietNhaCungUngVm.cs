namespace TPApp.ViewModel
{
    public class ChiTietNhaCungUngVm
    {
        // ===== Thông tin chính =====
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? DiaChi { get; set; }
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }

        public string? NguoiDaiDien { get; set; }
        public string? ChucVu { get; set; }

        public string? ChucNang { get; set; }
        public string? SanPham { get; set; }

        // ===== Hiển thị =====
        public string? ImageUrl { get; set; }
        public string? LinhVucText { get; set; }
        public string? DichVuText { get; set; }

        // ===== Thống kê =====
        public decimal Rating { get; set; }
        public int LuotDanhGia { get; set; }
        public int LuotXem { get; set; }

        // ===== Danh sách phụ =====
        public List<NhaCungUngItemVm> NhaCungUngKhac { get; set; } = new();
        public List<CategoryVm> Categories { get; set; } = new();
    }
    public class NhaCungUngItemVm
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? DiaChi { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }

        public int Rating { get; set; }
        public string? ImageUrl { get; set; }
    }
}
