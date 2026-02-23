using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace TPApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;
        private const int LogFunctionId = 10; // Users

        public UsersController(
            AppDbContext context,
            IAccountService accountService,
            IConfiguration configuration)
        {
            _context = context;
            _accountService = accountService;
            _configuration = configuration;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int id) ? id : 0;
        }

        private int GetSiteId() =>
            int.TryParse(_configuration["AppSettings:SiteId"], out var id) ? id : 1;

        private async Task WriteLog(int eventId, string content)
        {
            _context.Logs.Add(new Log
            {
                FunctionID = LogFunctionId,
                ActTime = DateTime.Now,
                EventID = eventId,
                Content = content,
                ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserName = User.Identity?.Name,
                Domain = HttpContext.Request.Host.Value,
                LanguageId = 1,
                ParentId = 0,
                SiteId = GetSiteId()
            });
            await _context.SaveChangesAsync();
        }

        // ─── INDEX (Search + Paged List) ───
        [HttpGet]
        public async Task<IActionResult> Index(
            string? userName, string? email, string? fullName, string? phone,
            int? isAdmin, bool? isActivated,
            DateTime? lastLoginFrom, DateTime? lastLoginTo,
            DateTime? createdFrom, DateTime? createdTo,
            int? siteId, string? sortBy, string? sortDir,
            int page = 1, int pageSize = 15)
        {
            // Default to current site if no filter specified
            if (!siteId.HasValue)
                siteId = GetSiteId();

            var query = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(userName))
                query = query.Where(u => u.UserName != null && u.UserName.Contains(userName));
            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => u.Email != null && u.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(fullName))
                query = query.Where(u => u.FullName != null && u.FullName.Contains(fullName));
            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(u => u.Phone != null && u.Phone.Contains(phone));
            if (isAdmin.HasValue)
                query = query.Where(u => u.IsAdmin == (isAdmin.Value == 1));
            if (isActivated.HasValue)
                query = query.Where(u => u.IsActivated == isActivated.Value);
            if (siteId.HasValue)
                query = query.Where(u => u.SiteId == siteId.Value);
            if (lastLoginFrom.HasValue)
                query = query.Where(u => u.LastLogin >= lastLoginFrom.Value);
            if (lastLoginTo.HasValue)
                query = query.Where(u => u.LastLogin <= lastLoginTo.Value.AddDays(1));
            if (createdFrom.HasValue)
                query = query.Where(u => u.Created >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(u => u.Created <= createdTo.Value.AddDays(1));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Dynamic sorting
            bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
            query = (sortBy?.ToLower()) switch
            {
                "username"    => asc ? query.OrderBy(u => u.UserName)     : query.OrderByDescending(u => u.UserName),
                "fullname"    => asc ? query.OrderBy(u => u.FullName)     : query.OrderByDescending(u => u.FullName),
                "phone"       => asc ? query.OrderBy(u => u.Phone)        : query.OrderByDescending(u => u.Phone),
                "email"       => asc ? query.OrderBy(u => u.Email)        : query.OrderByDescending(u => u.Email),
                "lastlogin"   => asc ? query.OrderBy(u => u.LastLogin)    : query.OrderByDescending(u => u.LastLogin),
                "isadmin"     => asc ? query.OrderBy(u => u.IsAdmin)      : query.OrderByDescending(u => u.IsAdmin),
                "created"     => asc ? query.OrderBy(u => u.Created)      : query.OrderByDescending(u => u.Created),
                "isactivated" => asc ? query.OrderBy(u => u.IsActivated)  : query.OrderByDescending(u => u.IsActivated),
                _             => query.OrderByDescending(u => u.Created)
            };

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new CmsUserListItem
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? "",
                    FullName = u.FullName ?? "",
                    Email = u.Email ?? "",
                    Phone = u.Phone ?? "",
                    LastLogin = u.LastLogin,
                    Created = u.Created,
                    IsActivated = u.IsActivated ?? false,
                    UserTypeId = u.UserTypeId,
                    IsAdmin = u.IsAdmin ?? false
                })
                .ToListAsync();

            // Lookup account type names (vAccountType.Id is nvarchar, Users.UserTypeId is int)
            var accountTypes = await _context.VAccountTypes.AsNoTracking().ToListAsync();
            var accountTypeMap = accountTypes.ToDictionary(a => a.Id, a => a.Name ?? "");
            foreach (var u in users)
            {
                var key = u.UserTypeId?.ToString() ?? "";
                u.AccountTypeName = accountTypeMap.ContainsKey(key)
                    ? accountTypeMap[key]
                    : "Thành viên";
            }

            ViewBag.AccountTypes = accountTypes;
            ViewBag.Sites = new SelectList(
                await _context.RootSites.AsNoTracking().ToListAsync(),
                "SiteId", "SiteName", siteId);
            ViewBag.CurrentSiteId = GetSiteId();

            // Preserve search params
            ViewBag.UserName = userName;
            ViewBag.Email = email;
            ViewBag.FullName = fullName;
            ViewBag.Phone = phone;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsActivated = isActivated;
            ViewBag.LastLoginFrom = lastLoginFrom;
            ViewBag.LastLoginTo = lastLoginTo;
            ViewBag.CreatedFrom = createdFrom;
            ViewBag.CreatedTo = createdTo;
            ViewBag.SiteId = siteId;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_UserListPartial", users);

            return View(users);
        }

        // ─── CREATE ───
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadFormLookups();
            return PartialView("_CreatePartial", new CmsUserCreateVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsUserCreateVm model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormLookups();
                return PartialView("_CreatePartial", model);
            }

            // Check duplicate username
            var exists = await _context.Users.AnyAsync(u =>
                u.UserName == model.UserName);
            if (exists)
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                await LoadFormLookups();
                return PartialView("_CreatePartial", model);
            }

            int.TryParse(model.SelectedAccountTypeId, out int parsedAcctType);
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                Domain = "techport.vn",
                Created = DateTime.Now,
                IsActivated = model.IsActivated,
                IsAdmin = model.SelectedAccountTypeId == "1",
                UserTypeId = parsedAcctType,
                SiteId = model.SelectedSiteId ?? 1,
                LanguageId = 1,
                ParentId = 0,
                IsUser = (byte)parsedAcctType
            };

            // Hash password before insert (Password column is NOT NULL)
            await _accountService.SetPasswordHashBeforeInsert(user, model.Password!);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Insert UserRoles
            if (model.SelectedRoleIds?.Any() == true)
            {
                foreach (var roleId in model.SelectedRoleIds)
                {
                    _context.CmsUserRoles.Add(new CmsUserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        Domain = "techport.vn",
                        LanguageId = 1,
                        ParentId = 0,
                        SiteId = user.SiteId
                    });
                }
                await _context.SaveChangesAsync();
            }

            await WriteLog(1, $"Create User: {model.UserName} (ID={user.Id})");
            return Json(new { success = true, message = $"Đã tạo người dùng '{model.UserName}' thành công." });
        }

        // ─── EDIT ───
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var model = new CmsUserEditVm
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                IsActivated = user.IsActivated ?? false,
                SelectedAccountTypeId = user.UserTypeId?.ToString(),
                SelectedSiteId = user.SiteId,
                SelectedRoleIds = await _context.CmsUserRoles
                    .Where(r => r.UserId == id)
                    .Select(r => r.RoleId).ToListAsync()
            };

            await LoadFormLookups();
            return PartialView("_EditPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CmsUserEditVm model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormLookups();
                return PartialView("_EditPartial", model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null) return NotFound();

            int.TryParse(model.SelectedAccountTypeId, out int parsedAcctType);
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.IsActivated = model.IsActivated;
            user.UserTypeId = parsedAcctType;
            user.IsAdmin = model.SelectedAccountTypeId == "1";
            user.SiteId = model.SelectedSiteId ?? user.SiteId;
            user.IsUser = (byte)parsedAcctType;

            _context.Users.Update(user);

            // Replace UserRoles
            var oldRoles = _context.CmsUserRoles.Where(r => r.UserId == model.UserId);
            _context.CmsUserRoles.RemoveRange(oldRoles);

            if (model.SelectedRoleIds?.Any() == true)
            {
                foreach (var roleId in model.SelectedRoleIds)
                {
                    _context.CmsUserRoles.Add(new CmsUserRole
                    {
                        UserId = model.UserId,
                        RoleId = roleId,
                        Domain = user.Domain,
                        LanguageId = 1,
                        ParentId = 0,
                        SiteId = user.SiteId
                    });
                }
            }

            await _context.SaveChangesAsync();

            await WriteLog(2, $"Update User: {user.UserName} (ID={model.UserId})");
            return Json(new { success = true, message = "Cập nhật người dùng thành công." });
        }

        // ─── RESET PASSWORD ───
        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return PartialView("_ResetPasswordPartial", new CmsResetPasswordVm
            {
                UserId = id,
                UserName = user.UserName ?? ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(CmsResetPasswordVm model)
        {
            if (!ModelState.IsValid) return PartialView("_ResetPasswordPartial", model);

            var success = await _accountService.SetPasswordAsync(model.UserId, model.NewPassword!);

            if (success)
            {
                return Json(new { success = true, message = $"Đã cập nhật mật khẩu cho '{model.UserName}'." });
            }

            ModelState.AddModelError("", "Không tìm thấy người dùng.");
            return PartialView("_ResetPasswordPartial", model);
        }

        // ─── DELETE ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (id == currentUserId)
                return Json(new { success = false, message = "Không thể xóa tài khoản đang đăng nhập." });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng." });

            // Remove mappings
            _context.CmsUserRoles.RemoveRange(_context.CmsUserRoles.Where(r => r.UserId == id));
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await WriteLog(3, $"Delete User: {user.UserName} (ID={id})");
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(List<int> ids)
        {
            var currentUserId = GetCurrentUserId();
            ids = ids.Where(i => i != currentUserId).ToList();

            if (!ids.Any())
                return Json(new { success = false, message = "Không có người dùng nào để xóa." });

            _context.CmsUserRoles.RemoveRange(_context.CmsUserRoles.Where(r => ids.Contains(r.UserId)));
            _context.Users.RemoveRange(_context.Users.Where(u => ids.Contains(u.Id)));
            await _context.SaveChangesAsync();

            await WriteLog(3, $"BulkDelete Users: {ids.Count} users (IDs={string.Join(",", ids)})");
            return Json(new { success = true, deleted = ids.Count });
        }

        // ─── HELPERS ───
        private async Task LoadFormLookups()
        {
            ViewBag.Roles = await _context.CmsRoles.AsNoTracking().ToListAsync();
            ViewBag.AccountTypes = await _context.VAccountTypes.AsNoTracking().ToListAsync();
            ViewBag.Sites = await _context.RootSites.AsNoTracking().ToListAsync();
        }
    }

    // ─── View Models ───
    public class CmsUserListItem
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime? LastLogin { get; set; }
        public DateTime? Created { get; set; }
        public bool IsActivated { get; set; }
        public int? UserTypeId { get; set; }
        public string AccountTypeName { get; set; } = "";
        public bool IsAdmin { get; set; }
    }

    public class CmsUserCreateVm
    {
        [Required(ErrorMessage = "Bắt buộc")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Bắt buộc")]
        [MinLength(6, ErrorMessage = "Tối thiểu 6 ký tự")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Bắt buộc")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public bool IsActivated { get; set; } = true;
        public string? SelectedAccountTypeId { get; set; }
        public int? SelectedSiteId { get; set; }
        public List<int>? SelectedRoleIds { get; set; }
    }

    public class CmsUserEditVm
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "Bắt buộc")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public bool IsActivated { get; set; }
        public string? SelectedAccountTypeId { get; set; }
        public int? SelectedSiteId { get; set; }
        public List<int>? SelectedRoleIds { get; set; }
    }

    public class CmsResetPasswordVm
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "Bắt buộc")]
        [MinLength(6, ErrorMessage = "Tối thiểu 6 ký tự")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không khớp")]
        public string? ConfirmPassword { get; set; }
    }
}
