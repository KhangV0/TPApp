using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("LiquidationReports")]
    public class LiquidationReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public int? EContractId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? GiaTriThanhToanConLai { get; set; }

        [StringLength(50)]
        public string? SoHoaDon { get; set; }

        public string? HoaDonFile { get; set; } // File path

        public bool SanDaChuyenTien { get; set; } = false;

        public bool HopDongClosed { get; set; } = false;

        public int StatusId { get; set; } = 1;

        public int? NguoiTao { get; set; } // int to match Users.UserId

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiSua { get; set; } // int to match Users.UserId

        public DateTime? NgaySua { get; set; }
    }
}
