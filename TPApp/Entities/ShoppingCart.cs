using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ShoppingCart")]
    public class ShoppingCart
    {
        [Key]
        public long RecordId { get; set; }

        [MaxLength(50)]
        public string? CartId { get; set; }

        public int? Quantity { get; set; }

        public long? ProductId { get; set; }

        public int? TypeId { get; set; }

        public int? StoreId { get; set; }

        public int? UserId { get; set; }

        public DateTime? DateCreated { get; set; }

        public int? StatusId { get; set; }

        [MaxLength(500)]
        public string? Domain { get; set; }

        public int? LanguageId { get; set; }

        public int? ParentId { get; set; }

        public int? SiteId { get; set; }
    }
}
