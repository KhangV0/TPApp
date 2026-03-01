using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("NhaTuVan")]
    public class NhaTuVan
    {
        [Key]
        public int TuVanId { get; set; }

        [Required]
        public string FullName { get; set; }

        public string? QueryString { get; set; }

        public string? Email { get; set; }

        [Required]
        public string DateOfBirth { get; set; }

        public string? HinhDaiDien { get; set; }

        [Required]
        public string DiaChi { get; set; }

        [Required]
        public string Phone { get; set; }

        public string? HocHam { get; set; }

        public string? CoQuan { get; set; }

        public string? ChucVu { get; set; }

        public string? LinhVucId { get; set; }

        public string? DichVu { get; set; }

        public string? KetQuaNghienCuu { get; set; }

        public DateTime? Created { get; set; }

        public string? CreatedBy { get; set; }

        public int? UserId { get; set; }

        public bool? IsActivated { get; set; }

        [Required]
        public string Domain { get; set; }

        public int? StatusId { get; set; }

        public DateTime? Modified { get; set; }

        public string? Modifier { get; set; }

        public int? Rating { get; set; }

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? Viewed { get; set; }

        public string? Keywords { get; set; }

        public int? SiteId { get; set; }

        // ── New fields (Phiếu thông tin NhaTuVan) ──
        public string? MaDinhDanh { get; set; }       // ORCID, Scholar, ResearchGate links
        public int? TongTrichDan { get; set; }
        public int? HIndex { get; set; }
        public string? QuaTrinhDaoTao { get; set; }    // HTML table
        public string? QuaTrinhCongTac { get; set; }   // HTML table
        public string? CongBoKhoaHoc { get; set; }     // HTML: sách + bài báo
        public string? SangChe { get; set; }           // HTML table
        public string? DuAnNghienCuu { get; set; }     // HTML table
        public string? KinhNghiem { get; set; }        // HTML table
        public string? HoSoDinhKem { get; set; }       // file paths / HTML
        [MaxLength(500)]
        public string? HiepHoiKhoaHoc { get; set; }
    }
}
