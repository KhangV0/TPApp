using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    /// <summary>
    /// Singleton snapshot of key platform metrics.
    /// Always has exactly one row with Id = 1.
    /// Updated by DashboardBackgroundService every 10 minutes.
    /// </summary>
    [Table("DashboardSnapshot")]
    public class DashboardSnapshot
    {
        [Key]
        public int Id { get; set; } = 1;

        // Products
        public int TotalProducts { get; set; }
        public int CongNgheCount { get; set; }
        public int ThietBiCount { get; set; }
        public int TriTueCount { get; set; }

        // Projects
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }

        // Suppliers (NhaCungUng)
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }

        // Consultants (NhaTuVan)
        public int TotalConsultants { get; set; }
        public int ActiveConsultants { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
