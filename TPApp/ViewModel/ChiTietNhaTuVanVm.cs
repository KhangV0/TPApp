namespace TPApp.ViewModel
{
    public class ChiTietNhaTuVanVm
    {
        public int Id { get; set; }
        public string? LinhVucId { get; set; }

        public string? FullName { get; set; }
        public string? DiaChi { get; set; }
        public string? NgaySinh { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? HocHam { get; set; }
        public string? CoQuan { get; set; }
        public string? ChucVu { get; set; }

        public int Rating { get; set; }
        public int LuotDanhGia { get; set; }
        public int LuotXem { get; set; }

        public string? ImageUrl { get; set; }

        public string? LinhVucText { get; set; }
        public string? DichVuText { get; set; }
        public string? KetQuaNghienCuu { get; set; }

        // New fields
        public string? MaDinhDanh { get; set; }
        public int? TongTrichDan { get; set; }
        public int? HIndex { get; set; }
        public string? QuaTrinhDaoTao { get; set; }
        public string? QuaTrinhCongTac { get; set; }
        public string? CongBoKhoaHoc { get; set; }
        public string? SangChe { get; set; }
        public string? DuAnNghienCuu { get; set; }
        public string? KinhNghiem { get; set; }
        public string? HoSoDinhKem { get; set; }
        public string? HiepHoiKhoaHoc { get; set; }

        public List<string> TuKhoa { get; set; } = new();
        public List<NhaTuVanItemVm> NhaTuVanKhac { get; set; } = new();
        public List<CategoryVm> Categories { get; set; } = new();
    }

    public class NhaTuVanItemVm
    {
        public int Id { get; set; }

        public string? FullName { get; set; }
        public string Url { get; set; }

        public string? ImageUrl { get; set; }

        public string? CoQuan { get; set; }

        public string? DiaChi { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public int Rating { get; set; }

        public int ViewCount { get; set; }

        /// <summary>
        /// Link SEO tới chi tiết nhà tư vấn
        /// </summary>
        public string DetailUrl =>
            $"/nha-tu-van-{Id}.html";
    }

}
