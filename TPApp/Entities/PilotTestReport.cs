using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("PilotTestReports")]
    public class PilotTestReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public int? EContractId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả thử nghiệm")]
        public string MoTaThuNghiem { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập kết quả thử nghiệm")]
        public string KetQuaThuNghiem { get; set; } = null!;

        public string? VanDePhatSinh { get; set; }

        public string? GiaiPhap { get; set; }

        public string? FileKetQua { get; set; } // File path (PDF, images, videos)

        public string? FileBaoCao { get; set; } // File path

        public bool DatYeuCau { get; set; } = false;

        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
