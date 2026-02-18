using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class ProjectMembersController : Controller
    {
        private readonly IProjectMemberService _memberService;
        private readonly ILogger<ProjectMembersController> _logger;

        public ProjectMembersController(
            IProjectMemberService memberService,
            ILogger<ProjectMembersController> logger)
        {
            _memberService = memberService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: /ProjectMembers/Index?projectId=7
        public async Task<IActionResult> Index(int projectId)
        {
            var userId = GetCurrentUserId();

            // Guard: Only project members can view
            var members = await _memberService.GetMembersAsync(projectId);
            if (!members.Any(m => m.UserId == userId))
            {
                _logger.LogWarning("User {UserId} attempted to view members of project {ProjectId} without access", userId, projectId);
                return Forbid();
            }

            ViewBag.ProjectId = projectId;
            ViewBag.IsBuyer = await _memberService.IsBuyerAsync(projectId, userId);
            ViewBag.AvailableConsultants = await _memberService.GetAvailableConsultantsAsync(projectId);

            return View(members);
        }

        // POST: /ProjectMembers/AddConsultant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultant(int projectId, int userId)
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                await _memberService.AddConsultantAsync(projectId, userId, currentUserId);

                _logger.LogInformation("User {CurrentUserId} added consultant {UserId} to project {ProjectId}", currentUserId, userId, projectId);

                // Return JSON for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã thêm consultant thành công!" });
                }

                TempData["SuccessMessage"] = "Đã thêm consultant thành công!";
                return RedirectToAction("Index", new { projectId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error adding consultant to project {ProjectId}", projectId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }
                
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { projectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding consultant to project {ProjectId}", projectId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm consultant. Vui lòng thử lại." });
                }
                
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm consultant. Vui lòng thử lại.";
                return RedirectToAction("Index", new { projectId });
            }
        }

        // POST: /ProjectMembers/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int projectId, int userId)
        {
            var currentUserId = GetCurrentUserId();

            try
            {
                await _memberService.RemoveMemberAsync(projectId, userId, currentUserId);

                _logger.LogInformation("User {CurrentUserId} removed member {UserId} from project {ProjectId}", currentUserId, userId, projectId);

                TempData["SuccessMessage"] = "Đã xóa thành viên thành công!";
                return RedirectToAction("Index", new { projectId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error removing member from project {ProjectId}", projectId);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { projectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error removing member from project {ProjectId}", projectId);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa thành viên. Vui lòng thử lại.";
                return RedirectToAction("Index", new { projectId });
            }
        }
    }
}
