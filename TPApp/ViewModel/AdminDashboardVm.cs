using System;
using System.Collections.Generic;

namespace TPApp.ViewModel
{
    // ─────────────────────────────────────────────────────────────
    // Admin Analytics Dashboard – View Models
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Root view model passed to the Admin Dashboard view.
    /// Snapshot is read from cache (1 query). Analytics are computed
    /// by the background service and stored server-side.
    /// </summary>
    public class AdminDashboardVm
    {
        // ── Snapshot KPIs ──
        public SnapshotKpiVm Snapshot { get; set; } = new();

        // ── Growth Trend (last 12 months) ──
        public List<MonthlyStatVm> MonthlyStats { get; set; } = new();

        // ── Analytics ──
        public List<TopCategoryVm>   TopCategories  { get; set; } = new();
        public List<TopSupplierVm>   TopSuppliers   { get; set; } = new();
        public List<TopConsultantVm> TopConsultants { get; set; } = new();

        // ── Status distributions ──
        public ProductStatusDistVm ProductStatus { get; set; } = new();

        // ── System Health ──
        public SystemHealthVm SystemHealth { get; set; } = new();

        // ── Filter state (passed back from form) ──
        public int? FilterYear  { get; set; }
        public int? FilterMonth { get; set; }

        // ── Computed helpers ──
        public DateTime LastUpdated { get; set; }
        public bool IsSnapshotStale => (DateTime.UtcNow - LastUpdated).TotalMinutes > 15;
    }

    // ─── Snapshot KPIs ───────────────────────────────────────────
    public class SnapshotKpiVm
    {
        public int TotalProducts    { get; set; }
        public int CongNgheCount    { get; set; }
        public int ThietBiCount     { get; set; }
        public int TriTueCount      { get; set; }

        public int TotalProjects    { get; set; }
        public int ActiveProjects   { get; set; }
        public int CompletedProjects{ get; set; }

        public int TotalSuppliers   { get; set; }
        public int ActiveSuppliers  { get; set; }

        public int TotalConsultants { get; set; }
        public int ActiveConsultants{ get; set; }

        // Derived KPIs
        public double ActiveProjectRate  => TotalProjects  > 0 ? Math.Round((double)ActiveProjects   / TotalProjects  * 100, 1) : 0;
        public double ActiveSupplierRate => TotalSuppliers > 0 ? Math.Round((double)ActiveSuppliers  / TotalSuppliers * 100, 1) : 0;
        public double ActiveConsultantRate => TotalConsultants > 0 ? Math.Round((double)ActiveConsultants / TotalConsultants * 100, 1) : 0;

        // Month-over-month growth (populated by service)
        public double ProductMomGrowth   { get; set; }
        public double ProjectMomGrowth   { get; set; }
        public double SupplierMomGrowth  { get; set; }
        public double ConsultantMomGrowth{ get; set; }
    }

    // ─── Monthly Growth Stats ─────────────────────────────────────
    public class MonthlyStatVm
    {
        public int Year         { get; set; }
        public int Month        { get; set; }
        public int NewProducts  { get; set; }
        public int NewProjects  { get; set; }
        public int NewSuppliers { get; set; }
        public int NewConsultants{ get; set; }

        /// <summary>Label for chart axis, e.g. "T2/2025"</summary>
        public string Label => $"T{Month}/{Year}";
    }

    // ─── Analytics: Top Rankings ──────────────────────────────────
    public class TopCategoryVm
    {
        public string CategoryName { get; set; } = string.Empty;
        public int    Count        { get; set; }
    }

    public class TopSupplierVm
    {
        public int    SupplierId   { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int    ProductCount { get; set; }
    }

    public class TopConsultantVm
    {
        public int    ConsultantId   { get; set; }
        public string ConsultantName { get; set; } = string.Empty;
        public int    ProjectCount   { get; set; }
    }

    // ─── Status Distributions ────────────────────────────────────
    public class ProductStatusDistVm
    {
        /// <summary>StatusId = 1 → New / Chờ duyệt</summary>
        public int NewCount       { get; set; }
        /// <summary>StatusId = 2 → Đang xử lý</summary>
        public int ProcessingCount{ get; set; }
        /// <summary>StatusId = 3 → Đã duyệt / Active</summary>
        public int ActiveCount    { get; set; }
        /// <summary>StatusId = 4 → Đã hoàn thành (off market)</summary>
        public int CompletedCount { get; set; }
    }

    // ─── System Health ────────────────────────────────────────────
    public class SystemHealthVm
    {
        public int PendingProducts    { get; set; }
        public int DelayedProjects    { get; set; }
        public int UnverifiedSuppliers{ get; set; }
        public int UnverifiedConsultants{ get; set; }

        // Traffic light helpers
        public string PendingProductsLevel     => ToLevel(PendingProducts,    10, 30);
        public string DelayedProjectsLevel     => ToLevel(DelayedProjects,     3, 10);
        public string UnverifiedSuppliersLevel => ToLevel(UnverifiedSuppliers, 5, 20);
        public string UnverifiedConsultantsLevel => ToLevel(UnverifiedConsultants, 5, 20);

        private static string ToLevel(int value, int warnThreshold, int critThreshold)
        {
            if (value == 0)               return "ok";
            if (value < warnThreshold)    return "ok";
            if (value < critThreshold)    return "warn";
            return "critical";
        }
    }
}
