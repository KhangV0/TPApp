using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ProposalSubmissions")]
    public class ProposalSubmission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? RFQId { get; set; }

        public int? ProjectId { get; set; }

        [Required(ErrorMessage = "Vui lòng tải lên giải pháp kỹ thuật")]
        public string GiaiPhapKyThuat { get; set; } = null!; // File path

        public decimal? BaoGiaSoBo { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian triển khai")]
        [StringLength(200)]
        public string ThoiGianTrienKhai { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng tải lên hồ sơ năng lực")]
        public string HoSoNangLucDinhKem { get; set; } = null!; // File path

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
