using System.Text.Json;
using System.Text.Json.Serialization;

namespace TPApp.ViewModel
{
    /// <summary>
    /// Enterprise Home Analytics ViewModel.
    /// Inherits exclusively from DashboardSnapshot + DashboardMonthlyStats.
    /// Max 2 DB queries, 5-min memory cache.
    /// </summary>
    public class HomeAnalyticsVm
    {
        // ── Totals (from DashboardSnapshot) ───────────────────────
        public int TotalProducts   { get; set; }
        public int TotalProjects   { get; set; }
        public int TotalSuppliers  { get; set; }
        public int TotalExperts    { get; set; }

        // ── Product type breakdown (from snapshot) ─────────────────
        public int CongNgheCount   { get; set; }
        public int ThietBiCount    { get; set; }
        public int TriTueCount     { get; set; }

        // ── MoM growth (int %, server-side calc from last 2 rows) ──
        public int MomProducts  { get; set; }
        public int MomProjects  { get; set; }
        public int MomSuppliers { get; set; }
        public int MomExperts   { get; set; }

        // ── Last 6 months series (for line chart) ─────────────────
        public List<string> MonthLabels     { get; set; } = new();
        public List<int>    SeriesProducts  { get; set; } = new();
        public List<int>    SeriesProjects  { get; set; } = new();
        public List<int>    SeriesSuppliers { get; set; } = new();
        public List<int>    SeriesExperts   { get; set; } = new();

        // ── Current-month counts (latest monthly row) ─────────────
        public int ThisMonthNewProducts  { get; set; }
        public int ThisMonthNewProjects  { get; set; }
        public int ThisMonthNewSuppliers { get; set; }

        // ── Metadata ──────────────────────────────────────────────
        public DateTime LastUpdated  { get; set; } = DateTime.UtcNow;
        public string   TopTypeName  { get; set; } = string.Empty;

        // ── Feature flag: advanced analytics enabled ───────────────
        public bool AdvancedAnalyticsEnabled { get; set; }

        // ── Insight strings (enterprise-neutral tone) ─────────────
        public string InsightVelocity { get; set; } = string.Empty;
        public string InsightProject  { get; set; } = string.Empty;
        public string InsightSupply   { get; set; } = string.Empty;

        // ── MoM display helpers (no negative %) ───────────────────
        /// <summary>Returns display string for MoM badge. Never shows red negative %.</summary>
        public static string MomBadge(int pct)
        {
            if (pct > 0)  return $"▲ +{pct}% MoM";
            if (pct < 0)  return "↓ so với tháng trước";
            return "Ổn định";
        }

        public static string MomCssClass(int pct) => pct >= 0 ? "up" : "flat";

        // ── JSON serialization for Chart.js inline data ───────────
        private static readonly JsonSerializerOptions _json = new() { DefaultIgnoreCondition = JsonIgnoreCondition.Never };

        public string MonthLabelsJson     => JsonSerializer.Serialize(MonthLabels, _json);
        public string GrowthSeriesJson    => JsonSerializer.Serialize(new
        {
            products  = SeriesProducts,
            projects  = SeriesProjects,
            suppliers = SeriesSuppliers,
            experts   = SeriesExperts
        }, _json);
        public string ProductTypeJson => JsonSerializer.Serialize(new
        {
            labels = new[] { "Công nghệ", "Thiết bị", "Trí tuệ nhân tạo" },
            values = new[] { CongNgheCount, ThietBiCount, TriTueCount }
        }, _json);
    }
}
