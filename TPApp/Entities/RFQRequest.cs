using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("RFQRequests")]
    public class RFQRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mã RFQ")]
        [StringLength(50)]
        public string MaRFQ { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập yêu cầu kỹ thuật")]
        public string YeuCauKyThuat { get; set; } = null!;

        public string? TieuChuanNghiemThu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hạn chót nộp hồ sơ")]
        public DateTime HanChotNopHoSo { get; set; }

        public int? ProjectId { get; set; }

        public bool DaGuiNhaCungUng { get; set; } = false;

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
