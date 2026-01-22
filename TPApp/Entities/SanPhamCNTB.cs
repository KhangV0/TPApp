using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("SanPhamCNTB")]
    public class SanPhamCNTB
    {
        [Key]
        public int ID { get; set; }

        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? QueryString { get; set; }
        public string? QuyTrinhHinhAnh { get; set; }
        public string? URL { get; set; }

        public bool? IsYoutube { get; set; }

        public int? XuatXuId { get; set; }
        public int? MucDoId { get; set; }

        public string? CategoryId { get; set; }

        public string? MoTa { get; set; }
        public string? ThongSo { get; set; }
        public string? UuDiem { get; set; }

        public double? OriginalPrice { get; set; }
        public double? SellPrice { get; set; }

        public string? Currency { get; set; }

        public string? GiaiThuong { get; set; }

        public int? NCUId { get; set; }

        public string? Khachhang { get; set; }

        public int? StoreId { get; set; }

        public bool? IsSellOff { get; set; }
        public bool? IsHot { get; set; }

        public int? StatusId { get; set; }

        public DateTime? PublishedDate { get; set; }

        public int? DaBan { get; set; }
        public int? TinhTrangHang { get; set; }
        public int? TongSo { get; set; }

        public string? XuatXu { get; set; }

        public int? TinhTP { get; set; }

        public string? DiaChi { get; set; }
        public string? Phone { get; set; }
        public string? PhoneOther { get; set; }
        public string? HoTen { get; set; }

        public string? YahooId { get; set; }
        public string? SkypeId { get; set; }
        public string? WebUrl { get; set; }

        public int LanguageId { get; set; }

        public DateTime? Modified { get; set; }
        public string? Modifier { get; set; }

        public DateTime? bEffectiveDate { get; set; }
        public DateTime? eEffectiveDate { get; set; }

        public DateTime? Created { get; set; }
        public string? Creator { get; set; }

        public int? TypeId { get; set; }

        public string? SoBang { get; set; }
        public DateTime? NgayCapBang { get; set; }
        public DateTime? ThoiHan { get; set; }

        public string? CoQuanChuTri { get; set; }
        public string? CoQuanChuQuan { get; set; }

        public int? LoaiDeTai { get; set; }
        public int? Rating { get; set; }

        public int? ParentId { get; set; }

        public string? MoTaNgan { get; set; }

        public int? Viewed { get; set; }

        public string? Keywords { get; set; }

        public int? SiteId { get; set; }
    }
}
