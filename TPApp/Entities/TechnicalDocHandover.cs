using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("TechnicalDocHandovers")]
    public class TechnicalDocHandover
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public int? EContractId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập danh mục hồ sơ")]
        public string DanhMucHoSo { get; set; } = null!; // JSON format

        public string? SourceCode { get; set; } // File path or repository URL

        public string? TaiLieuKyThuat { get; set; } // File path

        public string? TaiLieuHuongDanSuDung { get; set; } // File path

        public string? TaiLieuBaoTri { get; set; } // File path

        public string? Database { get; set; } // File path or connection info

        public string? GhiChu { get; set; }

        public bool DaBanGiaoDayDu { get; set; } = false;

        public DateTime? NgayBanGiao { get; set; }

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
