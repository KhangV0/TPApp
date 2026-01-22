using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? Email { get; set; }

        public DateTime? LastLogin { get; set; }

        public string? FullName { get; set; }

        public byte IsUser { get; set; }

        public DateTime? Created { get; set; }

        public bool? IsActivated { get; set; }

        public bool? IsShowHome { get; set; }

        public int? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Mobile { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? UserTypeId { get; set; }

        public bool? IsAdmin { get; set; }

        public int? TinhTP { get; set; }

        public string? DiaChi { get; set; }

        public string? MobileOrther { get; set; }

        public int? ThoiHan { get; set; }

        public int? TongPhi { get; set; }

        public int? Vip { get; set; }

        public DateTime? bActiveVip { get; set; }

        public string? Code { get; set; }

        public bool? ApproveVip { get; set; }

        public DateTime? DatePost { get; set; }

        public DateTime? DateUp { get; set; }

        public int? NumberPost { get; set; }

        public int? NumberUp { get; set; }

        [Required]
        public string Domain { get; set; } = null!;

        public string? Img { get; set; }

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }
    }
}
