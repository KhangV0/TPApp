using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("LegalReviewForms")]
    public class LegalReviewForm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public int? NegotiationFormId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập người thực hiện kiểm tra")]
        [StringLength(200)]
        public string NguoiKiemTra { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập kết quả kiểm tra")]
        public string KetQuaKiemTra { get; set; } = null!;

        public string? VanDePhapLy { get; set; }

        public string? DeXuatChinhSua { get; set; }

        public string? FileKiemTra { get; set; } // File path

        public bool DaDuyet { get; set; } = false;

        public DateTime? NgayKiemTra { get; set; }

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
