using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    /// <summary>
    /// Enterprise-grade Admin Dashboard Service.
    ///
    /// Architecture:
    ///   GetAdminDashboardAsync  → reads DashboardSnapshot (1 row) + last-12-month stats
    ///   RebuildSnapshotAsync    → called by background job every 10 minutes
    ///   RebuildMonthlyStatsAsync→ called by background job once per day
    ///
    /// All heavy GROUP BY / COUNT queries are isolated to the Rebuild methods.
    /// The controller action is always a single lightweight read.
    /// </summary>
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminDashboardService> _logger;

        public AdminDashboardService(AppDbContext db, ILogger<AdminDashboardService> logger)
        {
            _db     = db;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════════════
        // FAST PATH — called on every controller request
        // ══════════════════════════════════════════════════════════
        public async Task<AdminDashboardVm> GetAdminDashboardAsync(int? filterYear = null, int? filterMonth = null)
        {
            var vm = new AdminDashboardVm
            {
                FilterYear  = filterYear,
                FilterMonth = filterMonth,
                LastUpdated = DateTime.UtcNow
            };

            // 1. Read snapshot – silently degrade if table doesn't exist yet
            try
            {
                var snap = await _db.DashboardSnapshots
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == 1);

                if (snap != null)
                {
                    vm.LastUpdated = snap.UpdatedAt;
                    vm.Snapshot    = MapSnapshot(snap);
                }
                // else: table exists but no row yet – zeros are fine
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[Dashboard] Snapshot read failed (table may not exist yet): {Msg}", ex.Message);
                // vm.Snapshot stays at default zeros — page still renders
            }

            // 2. Monthly stats – last 12 months
            try
            {
                var last12 = await _db.DashboardMonthlyStats
                    .AsNoTracking()
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => m.Month)
                    .Take(12)
                    .ToListAsync();

                vm.MonthlyStats = last12
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .Select(m => new MonthlyStatVm
                    {
                        Year           = m.Year,
                        Month          = m.Month,
                        NewProducts    = m.NewProducts,
                        NewProjects    = m.NewProjects,
                        NewSuppliers   = m.NewSuppliers,
                        NewConsultants = m.NewConsultants
                    })
                    .ToList();

                ComputeMomGrowth(vm);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[Dashboard] Monthly stats read failed: {Msg}", ex.Message);
            }

            // 3. Analytics (Top-5s + status dist)
            try
            {
                await PopulateAnalyticsAsync(vm);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[Dashboard] Analytics query failed: {Msg}", ex.Message);
            }

            // 4. System health
            try
            {
                await PopulateHealthAsync(vm);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[Dashboard] Health query failed: {Msg}", ex.Message);
            }

            return vm;
        }

        // ══════════════════════════════════════════════════════════
        // BACKGROUND REBUILDS
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Rebuild the DashboardSnapshot row.
        /// Called every 10 minutes by DashboardBackgroundService.
        /// </summary>
        public async Task RebuildSnapshotAsync()
        {
            _logger.LogInformation("[Dashboard] Rebuilding snapshot at {Time}", DateTime.UtcNow);

            try
            {
                // All counts in a single round-trip using async parallel tasks
                var productsTask    = _db.SanPhamCNTBs.AsNoTracking().CountAsync();
                var congNgheTask    = _db.SanPhamCNTBs.AsNoTracking().CountAsync(p => p.ProductType == 1);
                var thietBiTask     = _db.SanPhamCNTBs.AsNoTracking().CountAsync(p => p.ProductType == 2);
                var triTueTask      = _db.SanPhamCNTBs.AsNoTracking().CountAsync(p => p.ProductType == 3);

                var projectsTask    = _db.Projects.AsNoTracking().CountAsync();
                var activeProjectsTask = _db.Projects.AsNoTracking().CountAsync(p => p.StatusId == 2);
                var completedProjectsTask = _db.Projects.AsNoTracking().CountAsync(p => p.StatusId == 3);

                var suppliersTask   = _db.NhaCungUngs.AsNoTracking().CountAsync();
                var activeSuppTask  = _db.NhaCungUngs.AsNoTracking().CountAsync(s => s.IsActivated == true);

                var consultantsTask = _db.NhaTuVans.AsNoTracking().CountAsync();
                var activeConsTask  = _db.NhaTuVans.AsNoTracking().CountAsync(c => c.IsActivated == true);

                await Task.WhenAll(
                    productsTask, congNgheTask, thietBiTask, triTueTask,
                    projectsTask, activeProjectsTask, completedProjectsTask,
                    suppliersTask, activeSuppTask,
                    consultantsTask, activeConsTask);

                var snap = await _db.DashboardSnapshots.FindAsync(1);
                if (snap == null)
                {
                    snap = new DashboardSnapshot { Id = 1 };
                    _db.DashboardSnapshots.Add(snap);
                }

                snap.TotalProducts     = await productsTask;
                snap.CongNgheCount     = await congNgheTask;
                snap.ThietBiCount      = await thietBiTask;
                snap.TriTueCount       = await triTueTask;
                snap.TotalProjects     = await projectsTask;
                snap.ActiveProjects    = await activeProjectsTask;
                snap.CompletedProjects = await completedProjectsTask;
                snap.TotalSuppliers    = await suppliersTask;
                snap.ActiveSuppliers   = await activeSuppTask;
                snap.TotalConsultants  = await consultantsTask;
                snap.ActiveConsultants = await activeConsTask;
                snap.UpdatedAt         = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                _logger.LogInformation("[Dashboard] Snapshot rebuilt successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Dashboard] Error rebuilding snapshot.");
            }
        }

        /// <summary>
        /// Rebuild monthly stats for the current month.
        /// Called once per day by DashboardBackgroundService.
        /// </summary>
        public async Task RebuildMonthlyStatsAsync()
        {
            _logger.LogInformation("[Dashboard] Rebuilding monthly stats at {Time}", DateTime.UtcNow);

            try
            {
                var now   = DateTime.UtcNow;
                int year  = now.Year;
                int month = now.Month;
                var from  = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var to    = from.AddMonths(1);

                // Count entities created this month
                var newProducts    = await _db.SanPhamCNTBs .AsNoTracking().CountAsync(p => p.Created    >= from && p.Created    < to);
                var newProjects    = await _db.Projects      .AsNoTracking().CountAsync(p => p.CreatedDate >= from && p.CreatedDate < to);
                var newSuppliers   = await _db.NhaCungUngs   .AsNoTracking().CountAsync(s => s.Created    >= from && s.Created    < to);
                var newConsultants = await _db.NhaTuVans     .AsNoTracking().CountAsync(c => c.Created    >= from && c.Created    < to);

                var row = await _db.DashboardMonthlyStats
                    .FirstOrDefaultAsync(m => m.Year == year && m.Month == month);

                if (row == null)
                {
                    row = new DashboardMonthlyStats { Year = year, Month = month };
                    _db.DashboardMonthlyStats.Add(row);
                }

                row.NewProducts    = newProducts;
                row.NewProjects    = newProjects;
                row.NewSuppliers   = newSuppliers;
                row.NewConsultants = newConsultants;
                row.CreatedAt      = now;

                await _db.SaveChangesAsync();
                _logger.LogInformation("[Dashboard] Monthly stats for {Y}/{M} rebuilt.", year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Dashboard] Error rebuilding monthly stats.");
            }
        }

        // ══════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════

        private static SnapshotKpiVm MapSnapshot(DashboardSnapshot snap) => new()
        {
            TotalProducts     = snap.TotalProducts,
            CongNgheCount     = snap.CongNgheCount,
            ThietBiCount      = snap.ThietBiCount,
            TriTueCount       = snap.TriTueCount,
            TotalProjects     = snap.TotalProjects,
            ActiveProjects    = snap.ActiveProjects,
            CompletedProjects = snap.CompletedProjects,
            TotalSuppliers    = snap.TotalSuppliers,
            ActiveSuppliers   = snap.ActiveSuppliers,
            TotalConsultants  = snap.TotalConsultants,
            ActiveConsultants = snap.ActiveConsultants,
        };

        private static void ComputeMomGrowth(AdminDashboardVm vm)
        {
            if (vm.MonthlyStats.Count < 2) return;

            var current  = vm.MonthlyStats[^1];
            var previous = vm.MonthlyStats[^2];

            vm.Snapshot.ProductMomGrowth    = GrowthPct(current.NewProducts,    previous.NewProducts);
            vm.Snapshot.ProjectMomGrowth    = GrowthPct(current.NewProjects,    previous.NewProjects);
            vm.Snapshot.SupplierMomGrowth   = GrowthPct(current.NewSuppliers,   previous.NewSuppliers);
            vm.Snapshot.ConsultantMomGrowth = GrowthPct(current.NewConsultants, previous.NewConsultants);
        }

        private static double GrowthPct(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round((double)(current - previous) / previous * 100, 1);
        }

        private async Task PopulateAnalyticsAsync(AdminDashboardVm vm)
        {
            // Top 5 Categories (by product count) — use in-memory join to avoid EF translation issues
            var catGroups = await _db.SanPhamCNTBCategories
                .AsNoTracking()
                .GroupBy(sc => sc.CatId)
                .Select(g => new { CatId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var catIds = catGroups.Select(g => g.CatId).ToList();
            var categories = await _db.Categories
                .AsNoTracking()
                .Where(c => catIds.Contains(c.CatId))
                .Select(c => new { c.CatId, c.Title })
                .ToListAsync();

            vm.TopCategories = catGroups
                .Select(g => new TopCategoryVm
                {
                    CategoryName = categories.FirstOrDefault(c => c.CatId == g.CatId)?.Title ?? $"Danh mục #{g.CatId}",
                    Count        = g.Count
                })
                .ToList();

            // Top 5 Suppliers by product count
            vm.TopSuppliers = await _db.SanPhamCNTBs
                .AsNoTracking()
                .Where(p => p.NCUId != null)
                .GroupBy(p => p.NCUId!.Value)
                .Select(g => new { NCUId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .Join(_db.NhaCungUngs.AsNoTracking(),
                      g   => g.NCUId,
                      ncu => ncu.CungUngId,
                      (g, ncu) => new TopSupplierVm
                      {
                          SupplierId   = ncu.CungUngId,
                          SupplierName = ncu.FullName  ?? "N/A",
                          ProductCount = g.Count
                      })
                .ToListAsync();

            // Top 5 Consultants by project count — in-memory join
            var consGroups = await _db.ProjectConsultants
                .AsNoTracking()
                .GroupBy(pc => pc.ConsultantId)
                .Select(g => new { ConsId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var consUserIds = consGroups.Select(g => g.ConsId).ToList();
            var consUsers = await _db.Users
                .AsNoTracking()
                .Where(u => consUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName, u.UserName })
                .ToListAsync();

            vm.TopConsultants = consGroups
                .Select(g =>
                {
                    var u = consUsers.FirstOrDefault(x => x.Id == g.ConsId);
                    return new TopConsultantVm
                    {
                        ConsultantId   = g.ConsId,
                        ConsultantName = u?.FullName ?? u?.UserName ?? $"User #{g.ConsId}",
                        ProjectCount   = g.Count
                    };
                })
                .ToList();

            // Product status distribution
            var statusGroups = await _db.SanPhamCNTBs
                .AsNoTracking()
                .Where(p => p.StatusId != null)
                .GroupBy(p => p.StatusId!.Value)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToListAsync();

            int GetCount(int sid) => statusGroups.FirstOrDefault(g => g.StatusId == sid)?.Count ?? 0;

            vm.ProductStatus = new ProductStatusDistVm
            {
                NewCount        = GetCount(1),
                ProcessingCount = GetCount(2),
                ActiveCount     = GetCount(3),
                CompletedCount  = GetCount(4)
            };
        }

        private async Task PopulateHealthAsync(AdminDashboardVm vm)
        {
            // Pending products: StatusId = 1 (Chờ duyệt)
            var pending       = _db.SanPhamCNTBs.AsNoTracking().CountAsync(p => p.StatusId == 1);
            // Delayed projects: Active but older than 90 days with no completion
            var cutoff        = DateTime.Now.AddDays(-90);
            var delayed       = _db.Projects.AsNoTracking()
                                    .CountAsync(p => p.StatusId == 2 && p.CreatedDate < cutoff);
            // Unverified suppliers: IsActivated is null or false
            var unvSuppliers  = _db.NhaCungUngs.AsNoTracking().CountAsync(s => s.IsActivated != true);
            // Unverified consultants
            var unvConsultants= _db.NhaTuVans  .AsNoTracking().CountAsync(c => c.IsActivated != true);

            await Task.WhenAll(pending, delayed, unvSuppliers, unvConsultants);

            vm.SystemHealth = new SystemHealthVm
            {
                PendingProducts       = await pending,
                DelayedProjects       = await delayed,
                UnverifiedSuppliers   = await unvSuppliers,
                UnverifiedConsultants = await unvConsultants
            };
        }
    }
}
