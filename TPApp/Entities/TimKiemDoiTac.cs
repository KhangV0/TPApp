using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("TimKiemDoiTac")]
    public class TimKiemDoiTac
    {
        [Key]
        public int TimDoiTacId { get; set; }

        [MaxLength(200)]
        public string? FullName { get; set; }

        [MaxLength(500)]
        public string? QueryString { get; set; }

        [MaxLength(500)]
        public string? DiaChi { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Fax { get; set; }

        [MaxLength(500)]
        public string? HinhDaiDien { get; set; }

        [MaxLength(50)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? CategoryId { get; set; }

        [MaxLength(500)]
        public string? TenDonVi { get; set; }

        [MaxLength(500)]
        public string? SanPhamId { get; set; }

        [MaxLength(200)]
        public string? TenSanPham { get; set; }

        public string? MoTa { get; set; }

        public string? HinhThuc { get; set; }

        public DateTime? Created { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public int? UserId { get; set; }

        public bool? IsActivated { get; set; }

        [MaxLength(500)]
        public string? Domain { get; set; }

        public int? StatusId { get; set; }

        public DateTime? Modified { get; set; }

        [MaxLength(50)]
        public string? Modifier { get; set; }

        public int? Rating { get; set; }

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? Viewed { get; set; }

        [MaxLength(4000)]
        public string? Keywords { get; set; }

        public int? SiteId { get; set; }
    }
}
