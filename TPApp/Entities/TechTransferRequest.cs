using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("TechTransferRequests")]
    public class TechTransferRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; } = null!;

        [StringLength(100)]
        public string? ChucVu { get; set; }

        [StringLength(200)]
        public string? DonVi { get; set; }

        [StringLength(200)]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập điện thoại")]
        [StringLength(20)]
        public string DienThoai { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập tên công nghệ")]
        [StringLength(200)]
        public string TenCongNghe { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mô tả nhu cầu")]
        public string MoTaNhuCau { get; set; } = null!;

        [StringLength(100)]
        public string? LinhVuc { get; set; }

        public decimal? NganSachDuKien { get; set; }

        public int? DuAnId { get; set; }

        public int StatusId { get; set; } = 1;

        [StringLength(450)]
        public string? NguoiTao { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? NguoiSua { get; set; }

        public DateTime? NgaySua { get; set; }
    }
}
