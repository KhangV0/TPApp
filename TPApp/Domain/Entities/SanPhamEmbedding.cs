using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Domain.Entities
{
    /// <summary>
    /// Represents a product embedding stored in the database.
    /// Embeddings are generated using OpenAI's text-embedding-3-small model (1536 dimensions).
    /// </summary>
    [Table("SanPhamEmbeddings")]
    public class SanPhamEmbedding
    {
        /// <summary>
        /// Product ID (foreign key to SanPhamCNTB.ID)
        /// </summary>
        [Key]
        public int SanPhamId { get; set; }

        /// <summary>
        /// Supplier ID (foreign key to NhaCungUng.CungUngId)
        /// </summary>
        [Required]
        public int NCUId { get; set; }

        /// <summary>
        /// JSON-serialized embedding vector (float array with 1536 dimensions)
        /// </summary>
        [Required]
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Embedding { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the embedding was last updated
        /// </summary>
        [Required]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
