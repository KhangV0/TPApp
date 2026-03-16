using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TPApp.Data;

namespace TPApp.Areas.Cms.Controllers
{
    /// <summary>
    /// Quản trị số liệu Dashboard công khai (Homepage, Hợp đồng, Kết nối)
    /// </summary>
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class DashboardDataController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;

        private static readonly Dictionary<string, string> DataFiles = new()
        {
            ["home"]       = "js/home-analytics-data.json",
            ["contract"]   = "js/contract-dashboard-data.json",
            ["connection"] = "js/connection-dashboard-data.json"
        };

        public DashboardDataController(IWebHostEnvironment env, AppDbContext context)
        {
            _env = env;
            _context = context;
        }

        // GET: /cms/DashboardData
        public IActionResult Index()
        {
            return View();
        }

        // GET: /cms/DashboardData/Load?key=home
        [HttpGet]
        public IActionResult Load(string key)
        {
            if (!DataFiles.TryGetValue(key ?? "", out var relativePath))
                return Json(new { error = $"Key '{key}' không hợp lệ" });

            var filePath = Path.Combine(_env.WebRootPath, relativePath);
            if (!System.IO.File.Exists(filePath))
                return Json(new { error = "File không tồn tại" });

            var json = System.IO.File.ReadAllText(filePath);
            return Content(json, "application/json");
        }

        // POST: /cms/DashboardData/Save?key=home
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Save(string key, [FromBody] JsonElement body)
        {
            if (!DataFiles.TryGetValue(key ?? "", out var relativePath))
                return Json(new { success = false, error = $"Key '{key}' không hợp lệ" });

            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            try
            {
                // Ensure directory exists
                var dir = Path.GetDirectoryName(filePath)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var backupPath = filePath + ".bak";
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Copy(filePath, backupPath, overwrite: true);

                var options = new JsonSerializerOptions { WriteIndented = true };
                var pretty = JsonSerializer.Serialize(body, options);
                await System.IO.File.WriteAllTextAsync(filePath, pretty);

                // Auto-sync home analytics stat boxes
                if (key == "contract" || key == "connection")
                {
                    await SyncHomeStatBoxes();
                }

                return Json(new { success = true, message = $"Đã cập nhật {key}", lastUpdated = DateTime.Now.ToString("dd/MM/yyyy HH:mm") });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, error = $"Không có quyền ghi file '{relativePath}'. Kiểm tra quyền Write cho IIS AppPool trên thư mục wwwroot/js." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi lưu file: {ex.GetType().Name} - {ex.Message}" });
            }
        }

        /// <summary>
        /// Đồng bộ statBoxes trong home-analytics-data.json từ contract + connection JSON
        /// </summary>
        private async Task SyncHomeStatBoxes()
        {
            var homePath = Path.Combine(_env.WebRootPath, DataFiles["home"]);
            var contractPath = Path.Combine(_env.WebRootPath, DataFiles["contract"]);
            var connectionPath = Path.Combine(_env.WebRootPath, DataFiles["connection"]);

            if (!System.IO.File.Exists(homePath)) return;

            var homeJson = await System.IO.File.ReadAllTextAsync(homePath);
            using var homeDoc = JsonDocument.Parse(homeJson);
            var homeObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(homeJson)!;

            // Read contract summary
            int totalContracts = 0, totalValue = 0;
            if (System.IO.File.Exists(contractPath))
            {
                var cJson = await System.IO.File.ReadAllTextAsync(contractPath);
                using var cDoc = JsonDocument.Parse(cJson);
                if (cDoc.RootElement.TryGetProperty("summary", out var cSummary))
                {
                    totalContracts = cSummary.TryGetProperty("totalContracts", out var sc) ? sc.GetInt32() : 0;
                    totalValue = cSummary.TryGetProperty("totalValue", out var tv) ? tv.GetInt32() : 0;
                }
            }

            // Read connection summary
            int totalConnections = 0;
            if (System.IO.File.Exists(connectionPath))
            {
                var nJson = await System.IO.File.ReadAllTextAsync(connectionPath);
                using var nDoc = JsonDocument.Parse(nJson);
                if (nDoc.RootElement.TryGetProperty("summary", out var nSummary))
                {
                    totalConnections = nSummary.TryGetProperty("totalConnections", out var tc) ? tc.GetInt32() : 0;
                }
            }

            // Update statBoxes array
            if (homeDoc.RootElement.TryGetProperty("statBoxes", out var statBoxes) && statBoxes.ValueKind == JsonValueKind.Array)
            {
                var boxes = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(statBoxes.GetRawText())!;
                foreach (var box in boxes)
                {
                    if (box.TryGetValue("label", out var label))
                    {
                        var lbl = label?.ToString() ?? "";
                        if (lbl == "Kết nối cung cầu") box["value"] = totalConnections;
                        else if (lbl == "Hợp đồng ký kết") box["value"] = totalContracts;
                        else if (lbl == "Tổng giá trị HĐ (triệu)") box["value"] = totalValue;
                    }
                }
                homeObj["statBoxes"] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(boxes));
            }

            // Write back
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var updated = JsonSerializer.Serialize(homeObj, opts);
            await System.IO.File.WriteAllTextAsync(homePath, updated);
        }

        // GET: /cms/DashboardData/FetchContracts — Lấy dữ liệu hợp đồng từ DB
        [HttpGet]
        public async Task<IActionResult> FetchContracts()
        {
            try
            {
                var contracts = await _context.EContracts
                    .Join(_context.Projects,
                        ec => ec.ProjectId,
                        p => p.Id,
                        (ec, p) => new { ec, p })
                    .GroupJoin(
                        _context.TechTransferRequests,
                        ep => ep.ec.ProjectId,
                        ttr => ttr.ProjectId,
                        (ep, ttrs) => new { ep.ec, ep.p, ttr = ttrs.FirstOrDefault() })
                    .Select(x => new
                    {
                        soHopDong = x.ec.SoHopDong,
                        tenDuAn = x.p.ProjectName,
                        linhVuc = x.ttr != null ? (x.ttr.LinhVuc ?? "Khác") : "Khác",
                        loaiHD = "Chuyển giao CN",
                        congNghe = "Công nghệ thông thường",
                        trangThai = x.ec.TrangThaiKy ?? "Chưa ký",
                        giaTriTrieu = 0,
                        ngayTao = x.ec.NgayTao.ToString("dd/MM/yyyy")
                    })
                    .ToListAsync();

                return Json(new { contracts });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi truy vấn DB: " + ex.Message });
            }
        }

        // GET: /cms/DashboardData/FetchConnections — Lấy dữ liệu kết nối từ DB
        [HttpGet]
        public async Task<IActionResult> FetchConnections()
        {
            try
            {
                var connections = await _context.NegotiationForms
                    .Join(_context.Projects,
                        nf => nf.ProjectId,
                        p => p.Id,
                        (nf, p) => new { nf, p })
                    .GroupJoin(
                        _context.TechTransferRequests,
                        np => np.nf.ProjectId,
                        ttr => ttr.ProjectId,
                        (np, ttrs) => new { np.nf, np.p, ttr = ttrs.FirstOrDefault() })
                    .Select(x => new
                    {
                        maKetNoi = "KN-" + x.nf.Id.ToString("D3"),
                        tenDuAn = x.p.ProjectName,
                        linhVuc = x.ttr != null ? (x.ttr.LinhVuc ?? "Khác") : "Khác",
                        benMua = x.ttr != null ? (x.ttr.DonVi ?? "N/A") : "N/A",
                        benBan = "N/A",
                        giaChot = (int)(x.nf.GiaChotCuoiCung ?? 0),
                        trangThai = x.nf.StatusId == 5 ? "Hoàn tất"
                                  : x.nf.StatusId == 4 ? "Đã ký 1 bên"
                                  : x.nf.StatusId == 3 ? "Chờ ký"
                                  : x.nf.StatusId == 2 ? "Đã thỏa giá"
                                  : "Nháp",
                        ngayTao = x.nf.NgayTao.ToString("dd/MM/yyyy")
                    })
                    .ToListAsync();

                // Also get totalRequests from TechTransferRequests
                var totalRequests = await _context.TechTransferRequests.CountAsync();
                return Json(new { connections, totalRequests });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi truy vấn DB: " + ex.Message });
            }
        }
    }
}
