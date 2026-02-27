using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TPApp.Data;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    /// <summary>
    /// Home Analytics Service.
    ///
    /// Architecture principle:
    ///   Read-Only Consumer of Admin Dashboard layer.
    ///   Inherits: DashboardSnapshot (Id=1) + DashboardMonthlyStats.
    ///   Never queries SanPhamCNTB / Projects / NhaCungUng / NhaTuVan directly.
    ///   Cached 5 min via IMemoryCache → zero DB load on popular home page.
    ///
    /// Future-ready:
    ///   AdvancedAnalyticsEnabled flag → expand with top-industry, TRL, geo heatmap.
    /// </summary>
    public class HomeAnalyticsService : IHomeAnalyticsService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HomeAnalyticsService> _logger;

        private const string CacheKey       = "home:analytics:v1";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public HomeAnalyticsService(
            AppDbContext db,
            IMemoryCache cache,
            ILogger<HomeAnalyticsService> logger)
        {
            _db     = db;
            _cache  = cache;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════════════
        // PUBLIC
        // ══════════════════════════════════════════════════════════

        public async Task<HomeAnalyticsVm> GetHomeAnalyticsAsync()
        {
            return await _cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                return await BuildVmAsync();
            }) ?? new HomeAnalyticsVm();
        }

        public async Task<object> GetHomeAnalyticsJsonAsync()
        {
            var vm = await GetHomeAnalyticsAsync();
            return new
            {
                totals = new
                {
                    products  = vm.TotalProducts,
                    projects  = vm.TotalProjects,
                    suppliers = vm.TotalSuppliers,
                    experts   = vm.TotalExperts
                },
                mom = new
                {
                    products  = vm.MomProducts,
                    projects  = vm.MomProjects,
                    suppliers = vm.MomSuppliers,
                    experts   = vm.MomExperts
                },
                monthlyGrowth = new
                {
                    labels    = vm.MonthLabels,
                    products  = vm.SeriesProducts,
                    projects  = vm.SeriesProjects,
                    suppliers = vm.SeriesSuppliers,
                    experts   = vm.SeriesExperts
                },
                productTypes = new
                {
                    labels = new[] { "Công nghệ", "Thiết bị", "Trí tuệ nhân tạo" },
                    values = new[] { vm.CongNgheCount, vm.ThietBiCount, vm.TriTueCount }
                },
                insights = new
                {
                    velocity = vm.InsightVelocity,
                    projects = vm.InsightProject,
                    supply   = vm.InsightSupply
                },
                topTypeName  = vm.TopTypeName,
                thisMonthNew = vm.ThisMonthNewProducts,
                lastUpdated  = vm.LastUpdated.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            };
        }

        // ══════════════════════════════════════════════════════════
        // PRIVATE — build VM from snapshot + monthly stats
        // ══════════════════════════════════════════════════════════

        private async Task<HomeAnalyticsVm> BuildVmAsync()
        {
            var vm = new HomeAnalyticsVm();

            try
            {
                // ── Query 1: Snapshot singleton ────────────────────────
                var snap = await _db.DashboardSnapshots
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == 1);

                if (snap != null)
                {
                    vm.TotalProducts  = snap.TotalProducts;
                    vm.TotalProjects  = snap.TotalProjects;
                    vm.TotalSuppliers = snap.TotalSuppliers;
                    vm.TotalExperts   = snap.TotalConsultants;
                    vm.CongNgheCount  = snap.CongNgheCount;
                    vm.ThietBiCount   = snap.ThietBiCount;
                    vm.TriTueCount    = snap.TriTueCount;
                    vm.LastUpdated    = snap.UpdatedAt;
                }

                // ── Query 2: Last 6 monthly stats ─────────────────────
                var last6 = await _db.DashboardMonthlyStats
                    .AsNoTracking()
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => m.Month)
                    .Take(6)
                    .ToListAsync();

                // Reverse to chronological order (oldest → newest)
                last6.Reverse();

                foreach (var row in last6)
                {
                    vm.MonthLabels    .Add($"T{row.Month}/{row.Year % 100:D2}");
                    vm.SeriesProducts .Add(row.NewProducts);
                    vm.SeriesProjects .Add(row.NewProjects);
                    vm.SeriesSuppliers.Add(row.NewSuppliers);
                    vm.SeriesExperts  .Add(row.NewConsultants);
                }

                // ── MoM (last 2 rows) ──────────────────────────────────
                if (last6.Count >= 2)
                {
                    var curr = last6[^1];
                    var prev = last6[^2];
                    vm.MomProducts  = MomPct(curr.NewProducts,    prev.NewProducts);
                    vm.MomProjects  = MomPct(curr.NewProjects,    prev.NewProjects);
                    vm.MomSuppliers = MomPct(curr.NewSuppliers,   prev.NewSuppliers);
                    vm.MomExperts   = MomPct(curr.NewConsultants, prev.NewConsultants);

                    // Current month absolute counts
                    vm.ThisMonthNewProducts  = curr.NewProducts;
                    vm.ThisMonthNewProjects  = curr.NewProjects;
                    vm.ThisMonthNewSuppliers = curr.NewSuppliers;
                }
                else if (last6.Count == 1)
                {
                    vm.ThisMonthNewProducts  = last6[0].NewProducts;
                    vm.ThisMonthNewProjects  = last6[0].NewProjects;
                    vm.ThisMonthNewSuppliers = last6[0].NewSuppliers;
                }

                // ── Derived fields ─────────────────────────────────────
                vm.TopTypeName = DetermineTopType(vm);
                BuildInsights(vm);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[HomeAnalytics] BuildVm failed: {Msg}", ex.Message);
                // Return vm with defaults — page always loads
            }

            return vm;
        }

        // ── Helpers ───────────────────────────────────────────────

        /// <summary>
        /// Returns MoM % clamped to a reasonable range for display.
        /// Intentionally returns int to keep display clean.
        /// </summary>
        private static int MomPct(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return (int)Math.Round((double)(current - previous) / previous * 100, 0);
        }

        private static string DetermineTopType(HomeAnalyticsVm vm)
        {
            int max = Math.Max(vm.CongNgheCount, Math.Max(vm.ThietBiCount, vm.TriTueCount));
            if (max == vm.CongNgheCount) return "Công nghệ";
            if (max == vm.ThietBiCount)  return "Thiết bị";
            return "Trí tuệ nhân tạo";
        }

        /// <summary>
        /// Generates enterprise-neutral insight strings.
        /// Never shows negative % — shows neutral context instead.
        /// </summary>
        private static void BuildInsights(HomeAnalyticsVm vm)
        {
            // Velocity — current month products
            vm.InsightVelocity = vm.ThisMonthNewProducts > 0
                ? $"{vm.ThisMonthNewProducts} sản phẩm mới tháng này"
                : "Đang cập nhật dữ liệu tháng mới";

            // Project — neutral even when mom is negative
            vm.InsightProject = vm.ThisMonthNewProjects > 0
                ? $"{vm.ThisMonthNewProjects} dự án khởi tạo mới"
                : "Tập trung triển khai dự án hiện tại";

            // Supply
            vm.InsightSupply = vm.ThisMonthNewSuppliers > 0
                ? $"{vm.ThisMonthNewSuppliers} nhà cung ứng mới tham gia"
                : "Nâng cao chất lượng hồ sơ nhà cung ứng";
        }
    }
}
