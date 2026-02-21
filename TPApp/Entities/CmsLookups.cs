using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Roles")]
    public class CmsRole
    {
        [Key]
        public int RoleId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }

    [Table("UserRole")]
    public class CmsUserRole
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Domain { get; set; } = "";
        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }

    [Table("RootSite")]
    public class RootSite
    {
        [Key]
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string? SiteName { get; set; }
        public string? DomainURL { get; set; }
        public string? Template { get; set; }
        public string? SourcePath { get; set; }
        public int? NguoiLienHeId { get; set; }
        public string? ThongTinLienHe { get; set; }
        public DateTime? Created { get; set; }
        public string? Creator { get; set; }
        public DateTime? Modified { get; set; }
        public string? Modifier { get; set; }
        public string Domain { get; set; } = "";
        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
    }

    [Table("vAccountType")]
    public class VAccountType
    {
        [Key]
        public string Id { get; set; } = "";
        public string? Name { get; set; }
        public string? Domain { get; set; }
        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }
}
