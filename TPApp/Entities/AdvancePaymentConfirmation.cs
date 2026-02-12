using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("AdvancePaymentConfirmations")]
    public class AdvancePaymentConfirmation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? EContractId { get; set; }

        public int? ProjectId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền tạm ứng")]
        public decimal SoTienTamUng { get; set; }

        public string? ChungTuChuyenTienFile { get; set; } // File path

        [Required(ErrorMessage = "Vui lòng chọn ngày chuyển")]
        public DateTime NgayChuyen { get; set; }

        public bool DaXacNhanNhanTien { get; set; } = false;

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
