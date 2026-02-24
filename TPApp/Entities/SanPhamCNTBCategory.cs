using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("SanPhamCNTBCategory")]
    public class SanPhamCNTBCategory
    {
        [Key]
        public int Id { get; set; }
        public int SanPhamCNTBId { get; set; }
        public int CatId { get; set; }
    }
}
