using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ImplementationLogs")]
    public class ImplementationLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public int? EContractId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giai đoạn")]
        [StringLength(100)]
        public string GiaiDoan { get; set; } = null!; // "Pilot" or "Triển khai đại trà"

        public string? KetQuaThucHien { get; set; }

        public string? HinhAnhVideoFile { get; set; } // File path

        public string? BienBanXacNhanFile { get; set; } // File path

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
