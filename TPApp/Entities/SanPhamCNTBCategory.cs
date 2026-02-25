using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    /// <summary>
    /// Maps dbo.SanPhamCNTBCategory — PK is composite (SanPhamCNTBId, CatId).
    /// Composite key is configured via Fluent API in AppDbContext.
    /// </summary>
    [Table("SanPhamCNTBCategory")]
    public class SanPhamCNTBCategory
    {
        public int SanPhamCNTBId { get; set; }
        public int CatId { get; set; }
    }
}
