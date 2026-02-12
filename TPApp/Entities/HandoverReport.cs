using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("HandoverReports")]
    public class HandoverReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? DuAnId { get; set; }

        public int? EContractId { get; set; }

        public string? DanhMucThietBiJson { get; set; }
        // store JSON array: [{Ten, Model, Serial, TinhTrang}]

        public string? DanhMucHoSoJson { get; set; }
        // store JSON array of selected checklist items

        public bool DaHoanThanhDaoTao { get; set; } = false;

        public int? DanhGiaSao { get; set; } // 1-5

        public string? NhanXet { get; set; }

        public int StatusId { get; set; } = 1;

        [StringLength(450)]
        public string? NguoiTao { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? NguoiSua { get; set; }

        public DateTime? NgaySua { get; set; }
    }
}
