using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("KeywordLienKet")]
    public class KeywordLienKet
    {
        [Key]
        public long Id { get; set; }

        public long? KeywordId { get; set; }

        public long? TargetId { get; set; }

        public int? TypeId { get; set; }

        [MaxLength(500)]
        public string? Tittle { get; set; }

        public string? Description { get; set; }

        public DateTime? Created { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [MaxLength(500)]
        public string? Img { get; set; }

        [MaxLength(500)]
        public string? Ref1 { get; set; }

        [MaxLength(500)]
        public string? Ref2 { get; set; }

        [MaxLength(500)]
        public string? Ref3 { get; set; }

        [MaxLength(500)]
        public string? Reft4 { get; set; }

        [MaxLength(500)]
        public string? Ref5 { get; set; }

        [MaxLength(50)]
        public string? Creator { get; set; }

        public short? Status { get; set; }

        public int? Sort { get; set; }

        public int? SiteId { get; set; }
    }
}
