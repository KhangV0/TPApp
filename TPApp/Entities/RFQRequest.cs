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

        [StringLength(450)]
        public string? NguoiTao { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? NguoiSua { get; set; }

        public DateTime? NgaySua { get; set; }
    }
}
