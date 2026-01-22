using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("NhaCungUng")]
    public class NhaCungUng
    {
        [Key]
        public int CungUngId { get; set; }

        [MaxLength(200)]
        public string? FullName { get; set; }

        [MaxLength(500)]
        public string? QueryString { get; set; }

        [MaxLength(500)]
        public string? HinhDaiDien { get; set; }

        [MaxLength(500)]
        public string? DiaChi { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Fax { get; set; }

        [MaxLength(50)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? NguoiDaiDien { get; set; }

        [MaxLength(50)]
        public string? ChucVu { get; set; }

        [MaxLength(500)]
        public string? LinhVucId { get; set; }

        public string? ChucNangChinh { get; set; }

        [MaxLength(500)]
        public string? DichVu { get; set; }

        public string? SanPham { get; set; }

        public DateTime? Created { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public int? UserId { get; set; }

        public bool? IsActivated { get; set; }

        [Required]
        [MaxLength(500)]
        public string Domain { get; set; } = string.Empty;

        public DateTime? Modified { get; set; }

        [MaxLength(50)]
        public string? Modifier { get; set; }

        public int? StatusId { get; set; }

        public int? Rating { get; set; }

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? Viewed { get; set; }

        [MaxLength(4000)]
        public string? Keywords { get; set; }

        public int? SiteId { get; set; }
    }
}
