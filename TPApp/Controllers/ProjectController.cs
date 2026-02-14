using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Services;

namespace TPApp.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Services.IWorkflowService _workflowService;

        public ProjectController(AppDbContext context, UserManager<ApplicationUser> userManager, Services.IWorkflowService workflowService)
        {
            _context = context;
            _userManager = userManager;
            _workflowService = workflowService;
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

        // GET: /Project/Workflow/5
        [HttpGet]
        public async Task<IActionResult> Workflow(int id)
        {
            var userId = GetCurrentUserId();
            var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == id && m.UserId == userId);
            if (!isMember) return Forbid();

            var steps = await _workflowService.GetProjectSteps(id);
            ViewBag.ProjectId = id;
            
            var project = await _context.Projects.FindAsync(id);
            ViewBag.ProjectName = project?.ProjectName;

            return View(steps);
        }

        // GET: /Project
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var projects = await _context.ProjectMembers
                .Where(m => m.UserId == userId)
                .Include(m => m.Project)
                .Select(m => m.Project)
                .ToListAsync();

            return View(projects);
        }

        // GET: /Project/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetCurrentUserId();
            var member = await _context.ProjectMembers
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId);

            if (member == null) return Forbid();

            // Get step statuses
            var statuses = await GetProjectStepStatuses(id.Value);

            // Build step navigation
            var steps = BuildStepNavigation(statuses);

            // Calculate current step (first incomplete step)
            var currentStep = 1;
            for (int i = 0; i < 11; i++)
            {
                if (steps[i].StatusId == 0)
                {
                    currentStep = i + 1;
                    break;
                }
                if (i == 10) currentStep = 11; // All completed
            }

            // Mark current step
            steps[currentStep - 1].IsCurrent = true;

            // Calculate progress
            var completedCount = statuses.Values.Count(s => s > 0);
            var progressPercent = (int)Math.Round((completedCount / 11.0) * 100);

            var viewModel = new TPApp.ViewModel.ProjectDetailWithStepsVm
            {
                Project = member.Project,
                Steps = steps,
                CurrentStep = currentStep,
                UserRole = member.Role,
                ProgressPercent = progressPercent
            };

            return View(viewModel);
        }

        // Helper: Get step statuses
        private async Task<Dictionary<string, int>> GetProjectStepStatuses(int projectId)
        {
            var statuses = new Dictionary<string, int>();

            var tech = await _context.TechTransferRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["TechTransfer"] = tech?.StatusId ?? 0;

            var nda = await _context.NDAAgreements.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["NDA"] = nda?.StatusId ?? 0;

            var rfq = await _context.RFQRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["RFQ"] = rfq?.StatusId ?? 0;

            var proposal = await _context.ProposalSubmissions.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Proposal"] = proposal?.StatusId ?? 0;

            var negotiation = await _context.NegotiationForms.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Negotiation"] = negotiation?.StatusId ?? 0;

            var contract = await _context.EContracts.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["EContract"] = contract?.StatusId ?? 0;

            var payment = await _context.AdvancePaymentConfirmations.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["AdvancePayment"] = payment?.StatusId ?? 0;

            var log = await _context.ImplementationLogs.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["ImplementationLog"] = log?.StatusId ?? 0;

            var handover = await _context.HandoverReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Handover"] = handover?.StatusId ?? 0;

            var acceptance = await _context.AcceptanceReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Acceptance"] = acceptance?.StatusId ?? 0;

            var liquidation = await _context.LiquidationReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Liquidation"] = liquidation?.StatusId ?? 0;

            return statuses;
        }

        // Helper: Build step navigation list
        private List<TPApp.ViewModel.ProjectStepNavVm> BuildStepNavigation(Dictionary<string, int> statuses)
        {
            var steps = new List<TPApp.ViewModel.ProjectStepNavVm>
            {
                new() { StepNumber = 1, StepName = "Yêu cầu chuyển giao", StatusId = statuses["TechTransfer"], ControllerName = "TechTransfer", ActionName = "Details", IsAccessible = true },
                new() { StepNumber = 2, StepName = "Thỏa thuận NDA", StatusId = statuses["NDA"], ControllerName = "NDA", ActionName = "Create", IsAccessible = statuses["TechTransfer"] > 0 },
                new() { StepNumber = 3, StepName = "Yêu cầu báo giá", StatusId = statuses["RFQ"], ControllerName = "RFQ", ActionName = "Create", IsAccessible = statuses["NDA"] > 0 },
                new() { StepNumber = 4, StepName = "Nộp hồ sơ", StatusId = statuses["Proposal"], ControllerName = "Proposal", ActionName = "Index", IsAccessible = statuses["RFQ"] > 0 },
                new() { StepNumber = 5, StepName = "Đàm phán", StatusId = statuses["Negotiation"], ControllerName = "Negotiation", ActionName = "Create", IsAccessible = statuses["Proposal"] > 0 },
                new() { StepNumber = 6, StepName = "Ký hợp đồng", StatusId = statuses["EContract"], ControllerName = "EContract", ActionName = "Create", IsAccessible = statuses["Negotiation"] > 0 },
                new() { StepNumber = 7, StepName = "Xác nhận tạm ứng", StatusId = statuses["AdvancePayment"], ControllerName = "AdvancePayment", ActionName = "Create", IsAccessible = statuses["EContract"] > 0 },
                new() { StepNumber = 8, StepName = "Nhật ký triển khai", StatusId = statuses["ImplementationLog"], ControllerName = "ImplementationLog", ActionName = "Create", IsAccessible = statuses["AdvancePayment"] > 0 },
                new() { StepNumber = 9, StepName = "Bàn giao", StatusId = statuses["Handover"], ControllerName = "Handover", ActionName = "Create", IsAccessible = statuses["ImplementationLog"] > 0 },
                new() { StepNumber = 10, StepName = "Nghiệm thu", StatusId = statuses["Acceptance"], ControllerName = "Acceptance", ActionName = "Create", IsAccessible = statuses["Handover"] > 0 },
                new() { StepNumber = 11, StepName = "Thanh lý", StatusId = statuses["Liquidation"], ControllerName = "Liquidation", ActionName = "Create", IsAccessible = statuses["Acceptance"] > 0 }
            };

            return steps;
        }

        // GET: /Project/Step - AJAX endpoint for loading step content
        [HttpGet]
        public async Task<IActionResult> Step(int projectId, int stepNumber)
        {
            // Validate project exists and user has access
            var userId = GetCurrentUserId();
            var member = await _context.ProjectMembers
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
            
            if (member == null) return Forbid();
            
            // Validate step number
            if (stepNumber < 1 || stepNumber > 11) return BadRequest("Invalid step number");
            
            // Get step statuses
            var statuses = await GetProjectStepStatuses(projectId);
            var steps = BuildStepNavigation(statuses);
            
            // Determine current step (first incomplete)
            var currentStep = 1;
            for (int i = 0; i < 11; i++)
            {
                if (steps[i].StatusId == 0)
                {
                    currentStep = i + 1;
                    break;
                }
                if (i == 10) currentStep = 11; // All complete
            }
            
            // Validate access - can't skip ahead
            if (stepNumber > currentStep)
            {
                // Return current step instead
                stepNumber = currentStep;
            }
            
            // Build model for partial view
            var model = new TPApp.ViewModel.StepContentVm
            {
                ProjectId = projectId,
                StepNumber = stepNumber,
                StepName = steps[stepNumber - 1].StepName,
                Project = member.Project,
                UserRole = member.Role,
                Steps = steps,
                CurrentStep = currentStep
            };
            
            // Load step-specific data for inline display
            model.StepData = await LoadStepData(projectId, stepNumber);
            
            // Return appropriate partial view
            return PartialView($"Steps/_Step{stepNumber}", model);
        }

        // Helper: Load step-specific data
        private async Task<object?> LoadStepData(int projectId, int stepNumber)
        {
            return stepNumber switch
            {
                1 => await _context.TechTransferRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                2 => await _context.NDAAgreements.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                3 => await _context.RFQRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                4 => await _context.ProposalSubmissions.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                5 => await _context.NegotiationForms.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                6 => await _context.EContracts.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                7 => await _context.AdvancePaymentConfirmations.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                8 => await _context.ImplementationLogs.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                9 => await _context.HandoverReports.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                10 => await _context.AcceptanceReports.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                11 => await _context.LiquidationReports.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                _ => null
            };
        }

        // POST: /Project/UpdateStepData - AJAX endpoint for updating step data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStepData(int projectId, int stepNumber, IFormCollection form)
        {
            try
            {
                // Validate project exists and user has access
                var userId = GetCurrentUserId();
                var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
                
                if (!isMember) return Json(new { success = false, message = "Không có quyền truy cập" });
                
                // Convert IFormCollection to Dictionary for helper methods
                var formData = form.Keys.ToDictionary(k => k, k => form[k].ToString());
                
                // Update based on step number
                bool success = stepNumber switch
                {
                    1 => await UpdateTechTransferData(projectId, formData, userId),
                    2 => await UpdateNDAData(projectId, formData, userId),
                    3 => await UpdateRFQData(projectId, formData, userId),
                    4 => await UpdateProposalData(projectId, formData, userId),
                    _ => false
                };
                
                if (success)
                {
                    return Json(new { success = true, message = "Cập nhật thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy dữ liệu để cập nhật" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Helper: Update TechTransfer data
        private async Task<bool> UpdateTechTransferData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.TechTransferRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            entity.HoTen = formData.GetValueOrDefault("HoTen", entity.HoTen);
            entity.ChucVu = formData.GetValueOrDefault("ChucVu");
            entity.DonVi = formData.GetValueOrDefault("DonVi");
            entity.DiaChi = formData.GetValueOrDefault("DiaChi");
            entity.DienThoai = formData.GetValueOrDefault("DienThoai", entity.DienThoai);
            entity.Email = formData.GetValueOrDefault("Email", entity.Email);
            entity.TenCongNghe = formData.GetValueOrDefault("TenCongNghe", entity.TenCongNghe);
            entity.LinhVuc = formData.GetValueOrDefault("LinhVuc");
            entity.MoTaNhuCau = formData.GetValueOrDefault("MoTaNhuCau", entity.MoTaNhuCau);
            
            if (formData.ContainsKey("NganSachDuKien") && decimal.TryParse(formData["NganSachDuKien"], out decimal budget))
            {
                entity.NganSachDuKien = budget;
            }

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update NDA data
        private async Task<bool> UpdateNDAData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.NDAAgreements.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            entity.BenA = formData.GetValueOrDefault("BenA", entity.BenA);
            entity.BenB = formData.GetValueOrDefault("BenB", entity.BenB);
            entity.LoaiNDA = formData.GetValueOrDefault("LoaiNDA", entity.LoaiNDA);
            entity.ThoiHanBaoMat = formData.GetValueOrDefault("ThoiHanBaoMat", entity.ThoiHanBaoMat);
            entity.XacNhanKySo = formData.GetValueOrDefault("XacNhanKySo");
            
            if (formData.ContainsKey("DaDongY") && bool.TryParse(formData["DaDongY"], out bool daDongY))
            {
                entity.DaDongY = daDongY;
            }

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update RFQ data
        private async Task<bool> UpdateRFQData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.RFQRequests.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            entity.MaRFQ = formData.GetValueOrDefault("MaRFQ", entity.MaRFQ);
            entity.YeuCauKyThuat = formData.GetValueOrDefault("YeuCauKyThuat", entity.YeuCauKyThuat);
            entity.TieuChuanNghiemThu = formData.GetValueOrDefault("TieuChuanNghiemThu");
            
            if (formData.ContainsKey("HanChotNopHoSo") && DateTime.TryParse(formData["HanChotNopHoSo"], out DateTime hanChot))
            {
                entity.HanChotNopHoSo = hanChot;
            }
            
            if (formData.ContainsKey("DaGuiNhaCungUng") && bool.TryParse(formData["DaGuiNhaCungUng"], out bool daGui))
            {
                entity.DaGuiNhaCungUng = daGui;
            }

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update Proposal data
        private async Task<bool> UpdateProposalData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.ProposalSubmissions.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            // Only update editable fields (not file paths)
            if (formData.ContainsKey("BaoGiaSoBo") && decimal.TryParse(formData["BaoGiaSoBo"], out decimal baoGia))
            {
                entity.BaoGiaSoBo = baoGia;
            }
            
            entity.ThoiGianTrienKhai = formData.GetValueOrDefault("ThoiGianTrienKhai", entity.ThoiGianTrienKhai);

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
