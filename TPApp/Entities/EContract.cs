using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("EContracts")]
    public class EContract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? RFQId { get; set; }

        public int? ProjectId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số hợp đồng")]
        [StringLength(50)]
        public string SoHopDong { get; set; } = null!;

        public string? FileHopDong { get; set; } // File path

        [StringLength(200)]
        public string? NguoiKyBenA { get; set; } // "Tên - Chức vụ - TokenKey"

        [StringLength(200)]
        public string? NguoiKyBenB { get; set; } // "Tên - Chức vụ - TokenKey"

        [StringLength(50)]
        public string TrangThaiKy { get; set; } = "Chưa ký"; // "Chưa ký", "Đã ký 1 bên", "Đã hoàn tất"

        public int StatusId { get; set; } = 1;

        [StringLength(450)]
        public string? NguoiTao { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? NguoiSua { get; set; }

        public DateTime? NgaySua { get; set; }
    }
}
