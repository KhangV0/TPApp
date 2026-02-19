using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPApp.Enums;

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

        // ── Version control ─────────────────────────────────────
        /// <summary>Version number starting at 1 (auto-incremented on upload revision)</summary>
        public int Version { get; set; } = 1;

        // ── Contract content ─────────────────────────────────────
        /// <summary>HTML snapshot generated from negotiation template</summary>
        public string? HtmlSnapshot { get; set; }

        /// <summary>Uploaded contract file path (PDF / DOCX)</summary>
        public string? ContractFilePath { get; set; }

        // ── Review data ──────────────────────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập người thực hiện kiểm tra")]
        [StringLength(200)]
        public string NguoiKiemTra { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập kết quả kiểm tra")]
        public string KetQuaKiemTra { get; set; } = string.Empty;

        public string? VanDePhapLy { get; set; }

        public string? DeXuatChinhSua { get; set; }

        public string? RejectionReason { get; set; }

        /// <summary>Legacy file field kept for backward compat</summary>
        public string? FileKiemTra { get; set; }

        // ── Approval ─────────────────────────────────────────────
        public bool DaDuyet { get; set; } = false;

        public int? ReviewedBy { get; set; }

        public DateTime? NgayKiemTra { get; set; }

        public DateTime? ReviewDeadline { get; set; }

        // ── Status ───────────────────────────────────────────────
        /// <summary>LegalReviewStatus enum value</summary>
        public int StatusId { get; set; } = (int)LegalReviewStatus.Draft;

        // ── Audit ────────────────────────────────────────────────
        public int? NguoiTao { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public int? NguoiSua { get; set; }
        public DateTime? NgaySua { get; set; }

        // ── Navigation ───────────────────────────────────────────
        [NotMapped]
        public List<ContractComment> Comments { get; set; } = new();
    }
}
