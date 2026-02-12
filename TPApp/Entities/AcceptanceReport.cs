using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("AcceptanceReports")]
    public class AcceptanceReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? DuAnId { get; set; }

        public int? EContractId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nghiệm thu")]
        public DateTime NgayNghiemThu { get; set; } = DateTime.Now;

        public string? ThanhPhanThamGia { get; set; }

        public string? KetLuanNghiemThu { get; set; }

        public string? VanDeTonDong { get; set; }

        [StringLength(450)]
        public string? ChuKyBenA { get; set; }

        [StringLength(450)]
        public string? ChuKyBenB { get; set; }

        [StringLength(50)]
        public string TrangThaiKy { get; set; } = "Chưa ký"; // "Chưa ký", "Đã ký 1 bên", "Hoàn tất"

        public int StatusId { get; set; } = 1;

        [StringLength(450)]
        public string? NguoiTao { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? NguoiSua { get; set; }

        public DateTime? NgaySua { get; set; }
    }
}
