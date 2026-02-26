using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TPApp.Controllers
{
    [Authorize]
    public class RFQController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;
        private readonly Services.INotificationQueueService _notifQueue;

        public RFQController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService, Services.INotificationQueueService notifQueue)
        {
            _context = context;
            _userManager = userManager;
            _workflowService = workflowService;
            _notifQueue = notifQueue;
        }

        // Helper method to get current user ID as int
        private int GetCurrentUserId()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID");
            }
            return userId;
        }

        // GET: /RFQ/Create?projectId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? projectId)
        {
             if (projectId == null) return NotFound("Project Id is required");

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Check Workflow Access (Step 3)
            if (!await _workflowService.CanAccessStep(projectId.Value, 3)) return Forbid();

            var existing = await _context.RFQRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (existing != null) return RedirectToAction("Details", "Project", new { id = projectId });

            // Auto-generate MaRFQ
            var maRFQ = "RFQ-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // Get TechTransfer's LinhVuc as default category filter
            var techTransfer = await _context.TechTransferRequests.AsNoTracking()
                .FirstOrDefaultAsync(t => t.ProjectId == projectId);
            var defaultLinhVuc = techTransfer?.LinhVuc ?? "";

            // Load categories for filter dropdown
            ViewBag.LinhVucList = await _context.Categories.AsNoTracking()
                .Where(c => c.ParentId == 1)
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem { Value = c.Title, Text = c.Title })
                .ToListAsync();
            ViewBag.DefaultLinhVuc = defaultLinhVuc;
            ViewBag.ProjectId = projectId;

            // Get already-invited seller user IDs for this project
            var invitedSellerIds = await _context.RFQInvitations
                .Where(i => i.ProjectId == projectId && i.IsActive)
                .Select(i => i.SellerId)
                .ToListAsync();
            ViewBag.InvitedSellerIds = invitedSellerIds;

            return View(new RFQRequest { ProjectId = projectId, MaRFQ = maRFQ });
        }

        // POST: /RFQ/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RFQRequest model, int[]? selectedSellerIds, bool alsoInvite = false)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == model.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            if (ModelState.IsValid)
            {
                // Set metadata
                model.NguoiTao = userId;
                model.NgayTao = DateTime.Now;
                model.StatusId = 1;
                model.DaGuiNhaCungUng = false;

                _context.RFQRequests.Add(model);
                await _context.SaveChangesAsync();

                // Complete Step 3
                await _workflowService.CompleteStep(model.ProjectId.Value, 3);

                // Notify buyer: RFQ created
                await _notifQueue.QueueAsync(userId, model.ProjectId,
                    "RFQ đã tạo",
                    "Yêu cầu báo giá đã tạo thành công. Hãy mời nhà cung ứng nộp hồ sơ.");

                // Also invite selected suppliers if requested
                if (alsoInvite && selectedSellerIds != null && selectedSellerIds.Length > 0)
                {
                    foreach (var sellerId in selectedSellerIds)
                    {
                        var existingInvitation = await _context.RFQInvitations
                            .FirstOrDefaultAsync(i => i.ProjectId == model.ProjectId &&
                                                     i.RFQId == model.Id &&
                                                     i.SellerId == sellerId);

                        if (existingInvitation == null)
                        {
                            _context.RFQInvitations.Add(new RFQInvitation
                            {
                                ProjectId = model.ProjectId.Value,
                                RFQId = model.Id,
                                SellerId = sellerId,
                                InvitedDate = DateTime.Now,
                                StatusId = 0,
                                IsActive = true
                            });

                            await _notifQueue.QueueAsync(sellerId, null,
                                "📨 Bạn được mời nộp hồ sơ báo giá",
                                $"Bạn được mời nộp hồ sơ báo giá. Hãy vào mục 'Dự án được mời' để xem chi tiết và nộp hồ sơ trước hạn.");
                        }
                    }

                    model.StatusId = 2; // Sent
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã tạo RFQ và mời {selectedSellerIds.Length} nhà cung ứng.";
                }

                return RedirectToAction("Details", "Project", new { id = model.ProjectId });
            }

            return View(model);
        }

        // GET: /RFQ/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var rfq = await _context.RFQRequests.FirstOrDefaultAsync(m => m.Id == id);
            if (rfq == null) return NotFound();

            return View(rfq);
        }

        // GET: /RFQ/SearchSuppliers?projectId=5&search=abc&linhVuc=xyz&page=1
        [HttpGet]
        public async Task<IActionResult> SearchSuppliers(int projectId, string? search, string? linhVuc, int page = 1)
        {
            const int pageSize = 10;

            // Get already-invited seller user IDs
            var invitedSellerIdsList = await _context.RFQInvitations
                .Where(i => i.ProjectId == projectId && i.IsActive)
                .Select(i => i.SellerId)
                .ToListAsync();
            var invitedSellerIds = invitedSellerIdsList.ToHashSet();

            // Query NhaCungUng with UserId, activated, StatusId=3, and LanguageId from session
            var lang = HttpContext.Session.GetInt32("LanguageId") ?? 1;
            var query = _context.NhaCungUngs.AsNoTracking()
                .Where(n => n.UserId != null && n.IsActivated == true
                    && n.StatusId == 3
                    && !invitedSellerIds.Contains(n.UserId!.Value)
                    && (n.LanguageId == null || n.LanguageId == lang));

            // Filter by lĩnh vực (resolve name to CatId, then search in ";id;" format)
            if (!string.IsNullOrWhiteSpace(linhVuc))
            {
                var catId = await _context.Categories.AsNoTracking()
                    .Where(c => c.Title == linhVuc && c.ParentId == 1)
                    .Select(c => c.CatId)
                    .FirstOrDefaultAsync();
                if (catId > 0)
                {
                    var catIdStr = catId.ToString();
                    query = query.Where(n => n.LinhVucId != null && n.LinhVucId.Contains(catIdStr));
                }
            }

            // Filter by search text (name)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(n =>
                    (n.FullName != null && n.FullName.ToLower().Contains(s)) ||
                    (n.Email != null && n.Email.ToLower().Contains(s)) ||
                    (n.Phone != null && n.Phone.Contains(s)));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var suppliers = await query
                .OrderBy(n => n.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    userId = n.UserId,
                    fullName = n.FullName ?? "",
                    diaChi = n.DiaChi ?? "",
                    email = n.Email ?? "",
                    phone = n.Phone ?? "",
                    linhVuc = n.LinhVucId ?? "",
                    userName = ""
                })
                .ToListAsync();

            // Resolve usernames
            var userIds = suppliers.Where(s => s.userId.HasValue).Select(s => s.userId!.Value).ToList();
            var userNames = await _context.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "");

            // Resolve LinhVucId (";1045;1064;") to category names
            var allCatIds = suppliers
                .SelectMany(s => s.linhVuc.Split(';', StringSplitOptions.RemoveEmptyEntries))
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();
            var catNames = allCatIds.Any()
                ? await _context.Categories.AsNoTracking()
                    .Where(c => allCatIds.Contains(c.CatId))
                    .ToDictionaryAsync(c => c.CatId, c => c.Title ?? "")
                : new Dictionary<int, string>();

            var data = suppliers.Select(s => new
            {
                s.userId,
                s.fullName,
                s.diaChi,
                s.email,
                s.phone,
                linhVuc = string.Join(", ", s.linhVuc
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Where(id => int.TryParse(id, out _))
                    .Select(int.Parse)
                    .Where(id => catNames.ContainsKey(id))
                    .Select(id => catNames[id])),
                userName = s.userId.HasValue && userNames.ContainsKey(s.userId.Value)
                    ? userNames[s.userId.Value] : ""
            });

            return Json(new { data, totalCount, totalPages, currentPage = page });
        }

        // POST: /RFQ/SendToSuppliers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToSuppliers(int id, int[] selectedSellerIds)
        {
            var rfq = await _context.RFQRequests.FindAsync(id);
            if (rfq == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == rfq.ProjectId && m.UserId == userId);
            if (!isMember) return Forbid();

            // Create invitation records for each selected seller
            if (selectedSellerIds != null && selectedSellerIds.Length > 0)
            {
                foreach (var sellerId in selectedSellerIds)
                {
                    // Check if invitation already exists
                    var existingInvitation = await _context.RFQInvitations
                        .FirstOrDefaultAsync(i => i.ProjectId == rfq.ProjectId && 
                                                 i.RFQId == id && 
                                                 i.SellerId == sellerId);

                    if (existingInvitation == null)
                    {
                        var invitation = new Entities.RFQInvitation
                        {
                            ProjectId = rfq.ProjectId.Value,
                            RFQId = id,
                            SellerId = sellerId,
                            InvitedDate = DateTime.Now,
                            StatusId = 0, // Invited
                            IsActive = true
                        };
                        _context.RFQInvitations.Add(invitation);

                        // Notify seller: invited to submit proposal (no projectId — seller has no access yet)
                        await _notifQueue.QueueAsync(sellerId, null,
                            "📨 Bạn được mời nộp hồ sơ báo giá",
                            $"Bạn được mời nộp hồ sơ báo giá. Hãy vào mục 'Dự án được mời' để xem chi tiết và nộp hồ sơ trước hạn.");
                    }
                    else if (!existingInvitation.IsActive)
                    {
                        // Reactivate existing invitation
                        existingInvitation.IsActive = true;
                        existingInvitation.InvitedDate = DateTime.Now;
                        existingInvitation.StatusId = 0; // Reset to Invited
                    }
                }
            }

            // Don't set DaGuiNhaCungUng flag - allow multiple sends
            // rfq.DaGuiNhaCungUng = true;
            rfq.StatusId = 2; // Sent status
            _context.Update(rfq);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã gửi RFQ đến {selectedSellerIds?.Length ?? 0} nhà cung ứng.";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = $"Đã mời {selectedSellerIds?.Length ?? 0} nhà cung ứng thành công!" });
            }

            return RedirectToAction("Details", "Project", new { id = rfq.ProjectId });
        }
    }
}
