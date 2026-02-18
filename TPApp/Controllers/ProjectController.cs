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
        private readonly Interfaces.IESignGateway _eSignGateway;

        public ProjectController(
            AppDbContext context, 
            UserManager<ApplicationUser> userManager, 
            Services.IWorkflowService workflowService,
            Interfaces.IESignGateway eSignGateway)
        {
            _context = context;
            _userManager = userManager;
            _workflowService = workflowService;
            _eSignGateway = eSignGateway;
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
            
            // Try to get project member first
            var member = await _context.ProjectMembers
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId);

            Project? project = null;
            int userRole = 0; // 0=Unknown, 1=Buyer, 2=Seller, 3=Consultant

            if (member != null)
            {
                // User is a project member
                project = member.Project;
                userRole = member.Role;

                // SECURITY CHECK: For Sellers (Role=2), verify invitation and NDA
                if (member.Role == 2)
                {
                    // Check invitation
                    var invitation = await _context.RFQInvitations
                        .FirstOrDefaultAsync(i => i.ProjectId == id &&
                                                 i.SellerId == userId &&
                                                 i.IsActive);
                    if (invitation == null)
                    {
                        TempData["ErrorMessage"] = "Bạn chưa được mời tham gia dự án này.";
                        return RedirectToAction("InvitedProjects", "Seller");
                    }

                    // Check NDA signature
                    var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(id.Value, userId);
                    if (!ndaSigned)
                    {
                        TempData["WarningMessage"] = "Bạn cần ký NDA trước khi xem chi tiết dự án.";
                        return RedirectToAction("SignNda", "Project", new { projectId = id });
                    }

                    // Update invitation status to Viewed if first time
                    if (invitation.StatusId == 0)
                    {
                        invitation.StatusId = 1; // Viewed
                        invitation.ViewedDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }

                    // Log seller access
                    await LogProjectAccessAsync(id.Value, userId, "ViewProject");
                }
            }
            else
            {
                // Not a project member - check if invited seller
                var invitation = await _context.RFQInvitations
                    .Include(i => i.Project)
                    .FirstOrDefaultAsync(i => i.ProjectId == id &&
                                             i.SellerId == userId &&
                                             i.IsActive);

                if (invitation != null)
                {
                    // User is an invited seller
                    project = invitation.Project;
                    userRole = 2; // Seller

                    // Check NDA signature
                    var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(id.Value, userId);
                    if (!ndaSigned)
                    {
                        TempData["WarningMessage"] = "Bạn cần ký NDA trước khi xem chi tiết dự án.";
                        return RedirectToAction("SignNda", "Project", new { projectId = id });
                    }

                    // Update invitation status to Viewed if first time
                    if (invitation.StatusId == 0)
                    {
                        invitation.StatusId = 1; // Viewed
                        invitation.ViewedDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }

                    // Log seller access
                    await LogProjectAccessAsync(id.Value, userId, "ViewProject");
                }
                else
                {
                    // Not a member and not invited - deny access
                    return Forbid();
                }
            }

            if (project == null) return NotFound();

            // Get step statuses
            var statuses = await GetProjectStepStatuses(id.Value);

            // Build step navigation
            var steps = BuildStepNavigation(statuses);

            // Calculate current step (first incomplete step)
            var currentStep = 1;
            for (int i = 0; i < 14; i++)
            {
                if (steps[i].StatusId == 0)
                {
                    currentStep = i + 1;
                    break;
                }
                if (i == 13) currentStep = 14; // All completed
            }

            // Mark current step
            steps[currentStep - 1].IsCurrent = true;

            // Calculate progress
            var completedCount = statuses.Values.Count(s => s > 0);
            var progressPercent = (int)Math.Round((completedCount / 14.0) * 100);

            var viewModel = new TPApp.ViewModel.ProjectDetailWithStepsVm
            {
                Project = project,
                Steps = steps,
                CurrentStep = currentStep,
                UserRole = userRole,
                ProgressPercent = progressPercent
            };

            return View(viewModel);
        }

        // GET: /Project/SignNda?projectId=7
        [HttpGet]
        public async Task<IActionResult> SignNda(int projectId)
        {
            var userId = GetCurrentUserId();

            // Guard 1: Check if user has invitation
            var invitation = await _context.RFQInvitations
                .Include(i => i.Project)
                .Include(i => i.RFQRequest)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && 
                                         i.SellerId == userId && 
                                         i.IsActive);

            if (invitation == null)
            {
                TempData["ErrorMessage"] = "Bạn chưa được mời tham gia dự án này.";
                return RedirectToAction("InvitedProjects", "Seller");
            }

            // Guard 2: Check if already signed
            var alreadySigned = await _eSignGateway.HasUserSignedProjectNda(projectId, userId);
            if (alreadySigned)
            {
                TempData["InfoMessage"] = "Bạn đã ký NDA cho dự án này rồi.";
                return RedirectToAction("InvitedProjects", "Seller");
            }

            // Guard 3: Check deadline
            if (invitation.RFQRequest?.HanChotNopHoSo < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Hạn chót nộp hồ sơ đã qua. Không thể ký NDA.";
                return RedirectToAction("InvitedProjects", "Seller");
            }

            // Pass data to view
            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = invitation.Project?.ProjectName;
            ViewBag.Deadline = invitation.RFQRequest?.HanChotNopHoSo;

            return View();
        }

        // POST: /Project/SignNda
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignNda(int projectId, string agreementText)
        {
            var userId = GetCurrentUserId();

            try
            {
                // Guard 1: Check invitation
                var invitation = await _context.RFQInvitations
                    .Include(i => i.RFQRequest)
                    .FirstOrDefaultAsync(i => i.ProjectId == projectId && 
                                             i.SellerId == userId && 
                                             i.IsActive);

                if (invitation == null)
                {
                    TempData["ErrorMessage"] = "Bạn chưa được mời tham gia dự án này.";
                    return RedirectToAction("InvitedProjects", "Seller");
                }

                // Guard 2: Check if already signed
                var alreadySigned = await _eSignGateway.HasUserSignedProjectNda(projectId, userId);
                if (alreadySigned)
                {
                    TempData["InfoMessage"] = "Bạn đã ký NDA cho dự án này rồi.";
                    return RedirectToAction("InvitedProjects", "Seller");
                }

                // Guard 3: Check deadline
                if (invitation.RFQRequest?.HanChotNopHoSo < DateTime.Now)
                {
                    TempData["ErrorMessage"] = "Hạn chót nộp hồ sơ đã qua.";
                    return RedirectToAction("InvitedProjects", "Seller");
                }

                // Create NDA document via E-Sign gateway
                var document = await _eSignGateway.CreateDocumentAsync(
                    projectId, 
                    1, // DocType: 1 = NDA
                    $"NDA - Project {projectId} - Seller {userId}",
                    userId
                );

                // Sign the document
                await _eSignGateway.SignDocumentAsync(
                    document.Id,
                    userId,
                    "Seller",
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    HttpContext.Request.Headers["User-Agent"].ToString()
                );

                // Log the action
                await LogProjectAccessAsync(projectId, userId, "SignNda", $"DocumentId: {document.Id}");

                TempData["SuccessMessage"] = "Bạn đã ký NDA thành công! Bây giờ bạn có thể chấp nhận lời mời.";
                return RedirectToAction("InvitedProjects", "Seller");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi khi ký NDA: {ex.Message}";
                return RedirectToAction("SignNda", new { projectId });
            }
        }

        // Helper: Log project access for audit trail
        private async Task LogProjectAccessAsync(int projectId, int userId, string action, string? additionalData = null)
        {
            var log = new TPApp.Entities.ProjectAccessLog
            {
                ProjectId = projectId,
                UserId = userId,
                Action = action,
                Timestamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                AdditionalData = additionalData
            };
            _context.ProjectAccessLogs.Add(log);
            await _context.SaveChangesAsync();
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

            var legalReview = await _context.LegalReviewForms.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["LegalReview"] = legalReview?.StatusId ?? 0;

            var contract = await _context.EContracts.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["EContract"] = contract?.StatusId ?? 0;

            var payment = await _context.AdvancePaymentConfirmations.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["AdvancePayment"] = payment?.StatusId ?? 0;

            var pilotTest = await _context.PilotTestReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["PilotTest"] = pilotTest?.StatusId ?? 0;

            var handover = await _context.HandoverReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Handover"] = handover?.StatusId ?? 0;

            var training = await _context.TrainingHandovers.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["Training"] = training?.StatusId ?? 0;

            var techDoc = await _context.TechnicalDocHandovers.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            statuses["TechDoc"] = techDoc?.StatusId ?? 0;

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
                new() { StepNumber = 1, StepName = "Yêu cầu chuyển giao công nghệ", StatusId = statuses["TechTransfer"], ControllerName = "TechTransfer", ActionName = "Details", IsAccessible = true },
                new() { StepNumber = 2, StepName = "Thỏa thuận bảo mật (NDA)", StatusId = statuses["NDA"], ControllerName = "NDA", ActionName = "Create", IsAccessible = statuses["TechTransfer"] > 0 },
                new() { StepNumber = 3, StepName = "Yêu cầu báo giá (RFQ)", StatusId = statuses["RFQ"], ControllerName = "RFQ", ActionName = "Create", IsAccessible = statuses["NDA"] > 0 },
                new() { StepNumber = 4, StepName = "Nộp hồ sơ đề xuất", StatusId = statuses["Proposal"], ControllerName = "Proposal", ActionName = "Index", IsAccessible = statuses["RFQ"] > 0 },
                new() { StepNumber = 5, StepName = "Đàm phán thương mại", StatusId = statuses["Negotiation"], ControllerName = "Negotiation", ActionName = "Create", IsAccessible = statuses["Proposal"] > 0 },
                new() { StepNumber = 6, StepName = "Kiểm tra pháp lý", StatusId = statuses["LegalReview"], ControllerName = "LegalReview", ActionName = "Create", IsAccessible = statuses["Negotiation"] > 0 },
                new() { StepNumber = 7, StepName = "Ký hợp đồng điện tử", StatusId = statuses["EContract"], ControllerName = "EContract", ActionName = "Create", IsAccessible = statuses["LegalReview"] > 0 },
                new() { StepNumber = 8, StepName = "Xác nhận tạm ứng", StatusId = statuses["AdvancePayment"], ControllerName = "AdvancePayment", ActionName = "Create", IsAccessible = statuses["EContract"] > 0 },
                new() { StepNumber = 9, StepName = "Thử nghiệm Pilot", StatusId = statuses["PilotTest"], ControllerName = "PilotTest", ActionName = "Create", IsAccessible = statuses["AdvancePayment"] > 0 },
                new() { StepNumber = 10, StepName = "Bàn giao & triển khai thiết bị", StatusId = statuses["Handover"], ControllerName = "Handover", ActionName = "Create", IsAccessible = statuses["PilotTest"] > 0 },
                new() { StepNumber = 11, StepName = "Đào tạo & chuyển giao vận hành", StatusId = statuses["Training"], ControllerName = "Training", ActionName = "Create", IsAccessible = statuses["Handover"] > 0 },
                new() { StepNumber = 12, StepName = "Bàn giao hồ sơ kỹ thuật", StatusId = statuses["TechDoc"], ControllerName = "TechDoc", ActionName = "Create", IsAccessible = statuses["Training"] > 0 },
                new() { StepNumber = 13, StepName = "Nghiệm thu", StatusId = statuses["Acceptance"], ControllerName = "Acceptance", ActionName = "Create", IsAccessible = statuses["TechDoc"] > 0 },
                new() { StepNumber = 14, StepName = "Thanh lý hợp đồng", StatusId = statuses["Liquidation"], ControllerName = "Liquidation", ActionName = "Create", IsAccessible = statuses["Acceptance"] > 0 }
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
            if (stepNumber < 1 || stepNumber > 14) return BadRequest("Invalid step number");
            
            // Get step statuses
            var statuses = await GetProjectStepStatuses(projectId);
            var steps = BuildStepNavigation(statuses);
            
            // Determine current step (first incomplete)
            var currentStep = 1;
            for (int i = 0; i < 14; i++)
            {
                if (steps[i].StatusId == 0)
                {
                    currentStep = i + 1;
                    break;
                }
                if (i == 13) currentStep = 14; // All complete
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
            
            // Special handling for Step 2 (NDA) - Load E-Sign data
            if (stepNumber == 2)
            {
                var eSignDoc = await _eSignGateway.GetProjectNdaAsync(projectId);
                ViewBag.ESignDocument = eSignDoc;
                
                if (eSignDoc != null)
                {
                    var signatures = await _eSignGateway.GetDocumentSignaturesAsync(eSignDoc.Id);
                    ViewBag.Signatures = signatures;
                }
            }
            
            // Special handling for Step 3 (RFQ) - Load available suppliers
            if (stepNumber == 3)
            {
                var availableSuppliers = await _context.NhaCungUngs
                    .Where(n => n.IsActivated == true && n.UserId != null)
                    .Select(n => new
                    {
                        n.CungUngId,
                        n.UserId,
                        n.FullName,
                        n.Email,
                        n.Phone,
                        n.DiaChi
                    })
                    .ToListAsync();
                ViewBag.AvailableSuppliers = availableSuppliers;
                
                // Load invited suppliers for this RFQ
        var rfqData = model.StepData as RFQRequest;
        if (rfqData != null)
        {
            var invitedSuppliers = await _context.RFQInvitations
                .Where(i => i.RFQId == rfqData.Id && i.IsActive)
                .Join(_context.NhaCungUngs,
                    inv => inv.SellerId,
                    ncc => ncc.UserId,
                    (inv, ncc) => new
                    {
                        inv.Id,
                        inv.SellerId,
                        inv.InvitedDate,
                        inv.StatusId,
                        inv.ViewedDate,
                        inv.ResponseDate,
                        ncc.FullName,
                        ncc.Email,
                        ncc.Phone
                    })
                .ToListAsync();
            
            // Check if each seller has submitted a proposal
            var invitedWithProposals = invitedSuppliers.Select(inv => new
            {
                inv.Id,
                inv.SellerId,
                inv.InvitedDate,
                inv.StatusId,
                inv.ViewedDate,
                inv.ResponseDate,
                inv.FullName,
                inv.Email,
                inv.Phone,
                HasProposal = _context.ProposalSubmissions
                    .Any(p => p.ProjectId == projectId && p.NguoiTao == inv.SellerId)
            }).ToList();
            
            ViewBag.InvitedSuppliers = invitedWithProposals;
        }
    }
            
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
                6 => await _context.LegalReviewForms.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                7 => await _context.EContracts.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                8 => await _context.AdvancePaymentConfirmations.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                9 => await _context.PilotTestReports.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                10 => await _context.HandoverReports.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                11 => await _context.TrainingHandovers.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                12 => await _context.TechnicalDocHandovers.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                13 => await _context.AcceptanceReports.FirstOrDefaultAsync(x => x.ProjectId == projectId),
                14 => await _context.LiquidationReports.FirstOrDefaultAsync(x => x.ProjectId == projectId),
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
                    5 => await UpdateNegotiationData(projectId, formData, userId),
                    6 => false, // TODO: Implement UpdateLegalReviewData
                    7 => await UpdateEContractData(projectId, formData, userId),
                    8 => await UpdateAdvancePaymentData(projectId, formData, userId),
                    9 => false, // TODO: Implement UpdatePilotTestData
                    10 => await UpdateHandoverData(projectId, formData, userId),
                    11 => false, // TODO: Implement UpdateTrainingData
                    12 => false, // TODO: Implement UpdateTechDocData
                    13 => await UpdateAcceptanceData(projectId, formData, userId),
                    14 => await UpdateLiquidationData(projectId, formData, userId),
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

        // Helper: Update Negotiation data
        private async Task<bool> UpdateNegotiationData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.NegotiationForms.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            // Update editable fields (not file path)
            if (formData.ContainsKey("GiaChotCuoiCung") && decimal.TryParse(formData["GiaChotCuoiCung"], out decimal gia))
            {
                entity.GiaChotCuoiCung = gia;
            }
            
            entity.DieuKhoanThanhToan = formData.GetValueOrDefault("DieuKhoanThanhToan", entity.DieuKhoanThanhToan);
            entity.HinhThucKy = formData.GetValueOrDefault("HinhThucKy", entity.HinhThucKy);
            
            if (formData.ContainsKey("DaKySo") && bool.TryParse(formData["DaKySo"], out bool daKy))
            {
                entity.DaKySo = daKy;
            }

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update E-Contract data
        private async Task<bool> UpdateEContractData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.EContracts.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            entity.SoHopDong = formData.GetValueOrDefault("SoHopDong", entity.SoHopDong);
            entity.NguoiKyBenA = formData.GetValueOrDefault("NguoiKyBenA", entity.NguoiKyBenA);
            entity.NguoiKyBenB = formData.GetValueOrDefault("NguoiKyBenB", entity.NguoiKyBenB);
            entity.TrangThaiKy = formData.GetValueOrDefault("TrangThaiKy", entity.TrangThaiKy);

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update Advance Payment data
        private async Task<bool> UpdateAdvancePaymentData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.AdvancePaymentConfirmations.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            if (formData.ContainsKey("SoTienTamUng") && decimal.TryParse(formData["SoTienTamUng"], out decimal soTien))
            {
                entity.SoTienTamUng = soTien;
            }

            if (formData.ContainsKey("NgayChuyen") && DateTime.TryParse(formData["NgayChuyen"], out DateTime ngay))
            {
                entity.NgayChuyen = ngay;
            }

            if (formData.ContainsKey("DaXacNhanNhanTien") && bool.TryParse(formData["DaXacNhanNhanTien"], out bool daXacNhan))
            {
                entity.DaXacNhanNhanTien = daXacNhan;
            }

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update Implementation data
        private async Task<bool> UpdateImplementationData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.ImplementationLogs.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            entity.GiaiDoan = formData.GetValueOrDefault("GiaiDoan", entity.GiaiDoan);
            entity.KetQuaThucHien = formData.GetValueOrDefault("KetQuaThucHien", entity.KetQuaThucHien);

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update Handover data
        private async Task<bool> UpdateHandoverData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.HandoverReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            entity.DanhMucThietBiJson = formData.GetValueOrDefault("DanhMucThietBiJson", entity.DanhMucThietBiJson);
            entity.DanhMucHoSoJson = formData.GetValueOrDefault("DanhMucHoSoJson", entity.DanhMucHoSoJson);
            entity.NhanXet = formData.GetValueOrDefault("NhanXet", entity.NhanXet);

            if (formData.ContainsKey("DaHoanThanhDaoTao") && bool.TryParse(formData["DaHoanThanhDaoTao"], out bool daHoanThanh))
            {
                entity.DaHoanThanhDaoTao = daHoanThanh;
            }

            if (formData.ContainsKey("DanhGiaSao") && int.TryParse(formData["DanhGiaSao"], out int sao))
            {
                entity.DanhGiaSao = sao;
            }

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update Acceptance data
        private async Task<bool> UpdateAcceptanceData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.AcceptanceReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            if (formData.ContainsKey("NgayNghiemThu") && DateTime.TryParse(formData["NgayNghiemThu"], out DateTime ngay))
            {
                entity.NgayNghiemThu = ngay;
            }

            entity.ThanhPhanThamGia = formData.GetValueOrDefault("ThanhPhanThamGia", entity.ThanhPhanThamGia);
            entity.KetLuanNghiemThu = formData.GetValueOrDefault("KetLuanNghiemThu", entity.KetLuanNghiemThu);
            entity.VanDeTonDong = formData.GetValueOrDefault("VanDeTonDong", entity.VanDeTonDong);
            entity.ChuKyBenA = formData.GetValueOrDefault("ChuKyBenA", entity.ChuKyBenA);
            entity.ChuKyBenB = formData.GetValueOrDefault("ChuKyBenB", entity.ChuKyBenB);
            entity.TrangThaiKy = formData.GetValueOrDefault("TrangThaiKy", entity.TrangThaiKy);

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // Helper: Update Liquidation data
        private async Task<bool> UpdateLiquidationData(int projectId, Dictionary<string, string> formData, int userId)
        {
            var entity = await _context.LiquidationReports.FirstOrDefaultAsync(x => x.ProjectId == projectId);
            if (entity == null) return false;

            if (formData.ContainsKey("GiaTriThanhToanConLai") && decimal.TryParse(formData["GiaTriThanhToanConLai"], out decimal giaTri))
            {
                entity.GiaTriThanhToanConLai = giaTri;
            }

            entity.SoHoaDon = formData.GetValueOrDefault("SoHoaDon", entity.SoHoaDon);

            if (formData.ContainsKey("SanDaChuyenTien") && bool.TryParse(formData["SanDaChuyenTien"], out bool daChuyenTien))
            {
                entity.SanDaChuyenTien = daChuyenTien;
            }

            if (formData.ContainsKey("HopDongClosed") && bool.TryParse(formData["HopDongClosed"], out bool closed))
            {
                entity.HopDongClosed = closed;
            }

            entity.NguoiSua = userId;
            entity.NgaySua = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
