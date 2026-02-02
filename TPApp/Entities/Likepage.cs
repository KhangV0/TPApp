using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Likepage")]
    public class Likepage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? IdPage { get; set; }
        public string? IPAddress { get; set; }
        public string? UserId { get; set; }
        public int? TypeID { get; set; }
        public int? Like { get; set; }
        public DateTime? Created { get; set; }
    }
}
