using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("NegotiationForms")]
    public class NegotiationForm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? RFQId { get; set; }

        public int? ProjectId { get; set; }

        public decimal? GiaChotCuoiCung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập điều khoản thanh toán")]
        public string DieuKhoanThanhToan { get; set; } = null!;

        public string? BienBanThuongLuongFile { get; set; } // File path

        [Required(ErrorMessage = "Vui lòng chọn hình thức ký")]
        [StringLength(50)]
        public string HinhThucKy { get; set; } = null!; // "Upload file", "E-Sign", "OTP"

        public bool DaKySo { get; set; } = false;

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
