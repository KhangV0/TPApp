using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    /// <summary>
    /// Monthly aggregated statistics for growth trend charts.
    /// One row per (Year, Month). Updated daily by DashboardBackgroundService.
    /// </summary>
    [Table("DashboardMonthlyStats")]
    public class DashboardMonthlyStats
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public int NewProducts { get; set; }
        public int NewProjects { get; set; }
        public int NewSuppliers { get; set; }
        public int NewConsultants { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
