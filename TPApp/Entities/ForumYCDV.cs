using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ForumYCDV")]
    public class ForumYCDV
    {
        [Key]
        public int ForumYCDVId { get; set; }

        public string? Title { get; set; }

        public string? NoiDung { get; set; }

        public string? HinhDaiDien { get; set; }

        public string? FullName { get; set; }

        public string? QueryString { get; set; }

        public string? DiaChi { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Fax { get; set; }

        public string? Website { get; set; }

        public string? TenDonVi { get; set; }

        public string? LinhVucId { get; set; }

        public string? DichVuId { get; set; }

        public DateTime? Created { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? LastModified { get; set; }

        public string? LastModifiedBy { get; set; }

        public int? UserId { get; set; }

        public string? IPAddress { get; set; }

        public bool? IsActivated { get; set; }

        public string? Domain { get; set; }

        public int? StatusId { get; set; }

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? Viewed { get; set; }

        public int? Like { get; set; }

        public int? Comment { get; set; }

        public int? ShareFB { get; set; }

        public int? SiteId { get; set; }
    }
}
