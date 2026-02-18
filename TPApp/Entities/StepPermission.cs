using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPApp.Enums;

namespace TPApp.Entities
{
    /// <summary>
    /// Defines permissions for each workflow step and role combination
    /// </summary>
    [Table("StepPermissions")]
    public class StepPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 14)]
        public int StepNumber { get; set; }

        [Required]
        public UserRoleType RoleType { get; set; }

        public bool CanView { get; set; }

        public bool CanEdit { get; set; }

        public bool CanSubmit { get; set; }

        public bool CanApprove { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
