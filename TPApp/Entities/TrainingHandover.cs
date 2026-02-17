using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("TrainingHandovers")]
    public class TrainingHandover
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public int? EContractId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đào tạo")]
        public string NoiDungDaoTao { get; set; } = null!;

        public string? DanhSachNguoiThamGia { get; set; } // JSON or text

        public int? SoNguoiThamGia { get; set; }

        public int? SoGioDaoTao { get; set; }

        public string? TaiLieuDaoTao { get; set; } // File path

        public string? VideoHuongDan { get; set; } // File path

        public string? BienBanDaoTao { get; set; } // File path

        public bool DaHoanThanh { get; set; } = false;

        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
