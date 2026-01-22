using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("CommentsYCTB")]
    public class CommentsYCTB
    {
        [Key]
        public long CommentYCTBId { get; set; }

        public long? ParentId { get; set; }

        public long TargetId { get; set; }

        public byte CommentTypeId { get; set; }

        public int? MemberId { get; set; }

        public string? Name { get; set; }

        public string Contents { get; set; } = null!;

        public string? Email { get; set; }

        public DateTime Created { get; set; }

        public string? IPAddress { get; set; }

        public byte? StatusId { get; set; }

        public string? UrlRefer { get; set; }

        public int? Like { get; set; }

        public int? Share { get; set; }

        public int? LanguageId { get; set; }

        public int? SiteId { get; set; }
    }
}
