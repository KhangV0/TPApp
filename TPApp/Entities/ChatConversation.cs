using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("ChatConversations")]
    public class ChatConversation
    {
        [Key]
        public long Id { get; set; }

        /// <summary>Link to SanPhamCNTB.ID (nullable for general chats)</summary>
        public int? ProductId { get; set; }

        /// <summary>1=CongNghe, 2=ThietBi, 3=SanPhamTriTue</summary>
        public int? ProductType { get; set; }

        [MaxLength(450)]
        public string BuyerUserId { get; set; } = string.Empty;

        [MaxLength(450)]
        public string SupplierUserId { get; set; } = string.Empty;

        /// <summary>Product name snapshot for display</summary>
        [MaxLength(500)]
        public string? ProductName { get; set; }

        /// <summary>Originated from product detail page</summary>
        public bool IsFromProductDetail { get; set; } = true;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? LastMessageAt { get; set; }
    }
}
