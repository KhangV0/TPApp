using System.ComponentModel.DataAnnotations;

namespace TPApp.Areas.Cms.Models
{
    /// <summary>
    /// ViewModel for Content Create / Edit form.
    /// </summary>
    public class ContentFormVm
    {
        public long Id { get; set; }

        // ── Tab 1: General ──
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(500)]
        public string Title { get; set; } = "";

        [StringLength(500)]
        public string? QueryString { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn menu")]
        public int? MenuId { get; set; }

        public int? TypeId { get; set; }

        [StringLength(200)]
        public string? Author { get; set; }

        // ── Tab 2: Editor ──
        public string? Contents { get; set; }

        // ── Tab 3: SEO ──
        [StringLength(500)]
        public string? Subject { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Keyword { get; set; }

        [StringLength(300)]
        public string? ImageBig { get; set; }

        // ── Tab 4: Publish ──
        public int? StatusId { get; set; }

        public DateTime? PublishedDate { get; set; }
        public DateTime? bEffectiveDate { get; set; }
        public DateTime? eEffectiveDate { get; set; }

        public bool IsHot { get; set; }
        public bool IsNew { get; set; }

        // ── Tab 5: Media ──
        [StringLength(300)]
        public string? Image { get; set; }

        public string? URL { get; set; }
        public bool IsYoutube { get; set; }
        public string? LinkFile { get; set; }
        public string? LinkInvite { get; set; }

        // ── Hidden / Auto ──
        public string Domain { get; set; } = "";
        public int LanguageId { get; set; } = 1;
        public int? SiteId { get; set; }
        public int? ParentId { get; set; }

        // Extra fields
        public string? ChuongTrinh { get; set; }
        public string? PhieuDangKy { get; set; }
        public string? STINFO { get; set; }
        public string? HinhSTINFO { get; set; }
    }
}
