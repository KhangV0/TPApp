using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Controllers
{
    [Authorize]
    public class ScoringController : Controller
    {
        private readonly IScoringService _scoringService;
        private readonly ILogger<ScoringController> _logger;

        public ScoringController(
            IScoringService scoringService,
            ILogger<ScoringController> logger)
        {
            _scoringService = scoringService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // POST: /Scoring/ScoreProposal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScoreProposal(ProposalScoreDto dto)
        {
            var userId = GetCurrentUserId();

            try
            {
                if (!await _scoringService.CanScoreProposalAsync(dto.ProposalId, userId))
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Bạn không thể chấm điểm báo giá này. Vui lòng kiểm tra quyền truy cập." });
                    }
                    
                    TempData["ErrorMessage"] = "Bạn không thể chấm điểm báo giá này. Vui lòng kiểm tra quyền truy cập.";
                    return RedirectToAction("Index", "ProposalList", new { projectId = dto.ProjectId });
                }

                await _scoringService.ScoreProposalAsync(dto.ProposalId, userId, dto);

                _logger.LogInformation("User {UserId} scored proposal {ProposalId}", userId, dto.ProposalId);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã chấm điểm thành công!" });
                }

                TempData["SuccessMessage"] = "Đã chấm điểm thành công!";
                return RedirectToAction("Details", "Project", new { id = dto.ProjectId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error scoring proposal {ProposalId}", dto.ProposalId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }
                
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "ProposalList", new { projectId = dto.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error scoring proposal {ProposalId}", dto.ProposalId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Đã xảy ra lỗi khi chấm điểm. Vui lòng thử lại." });
                }
                
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi chấm điểm. Vui lòng thử lại.";
                return RedirectToAction("Index", "ProposalList", new { projectId = dto.ProjectId });
            }
        }
    }
}
