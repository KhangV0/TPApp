using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPApp.ViewModel;

namespace TPApp.Entities
{
    [Table("Contents")]
    public class Content
    {
        [Key]
        public long Id { get; set; }

        public string? Title { get; set; }

        public string? QueryString { get; set; }

        public string? Description { get; set; }

        public string? Contents { get; set; }

        public string? ChuongTrinh { get; set; }

        public string? PhieuDangKy { get; set; }

        public string? STINFO { get; set; }

        public string? HinhSTINFO { get; set; }

        public string? Author { get; set; }

        public int? StatusId { get; set; }

        public int? MenuId { get; set; }

        public bool? IsHot { get; set; }

        public DateTime? Created { get; set; }

        public string? Creator { get; set; }

        public DateTime? Modified { get; set; }

        public string? Modifier { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string? Image { get; set; }

        public DateTime? PublishedDate { get; set; }

        public DateTime? bEffectiveDate { get; set; }

        public DateTime? eEffectiveDate { get; set; }

        public bool? IsNew { get; set; }

        public string? Subject { get; set; }

        public string? Keyword { get; set; }

        public int? Like { get; set; }

        public int? DisLike { get; set; }

        public int? Viewed { get; set; }

        public int? TypeId { get; set; }

        public string? URL { get; set; }

        public bool? IsYoutube { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string? ImageBig { get; set; }

        // NOT NULL
        public int LanguageId { get; set; }

        // NOT NULL
        public string Domain { get; set; } = null!;

        public string? LinkInvite { get; set; }

        public string? LinkFile { get; set; }

        public DateTime? DemoDate { get; set; }

        public bool? IsReport { get; set; }

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }

        public static implicit operator Content(VideoVm v)
        {
            throw new NotImplementedException();
        }
    }
}
