using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.ViewComponents
{
    public class ContractManagerViewComponent : ViewComponent
    {
        private readonly AppDbContext             _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContractService        _contracts;
        private readonly IContractApprovalService _approvals;

        public ContractManagerViewComponent(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IContractService contracts,
            IContractApprovalService approvals)
        {
            _context     = context;
            _userManager = userManager;
            _contracts   = contracts;
            _approvals   = approvals;
        }

        public async Task<IViewComponentResult> InvokeAsync(int projectId)
        {
            // Determine current user and role
            var userIdStr = _userManager.GetUserId(UserClaimsPrincipal);
            int userId    = int.TryParse(userIdStr, out int uid) ? uid : 0;

            var proj = await _context.Projects.FindAsync(projectId);
            int role = 0;
            if (proj != null)
            {
                if (proj.CreatedBy       == userId) role = 1; // Buyer
                else if (proj.SelectedSellerId == userId) role = 2; // Seller
                else
                {
                    bool isConsultant = await _context.ProjectConsultants
                        .AnyAsync(c => c.ProjectId == projectId && c.ConsultantId == userId);
                    if (isConsultant) role = 3;
                }
            }

            var active    = await _contracts.GetActiveContractAsync(projectId);
            var versions  = await _contracts.GetAllVersionsAsync(projectId);
            var approvals = active != null
                ? await _approvals.GetApprovalSummaryAsync(active.Id)
                : new List<ContractApproval>();
            bool allApproved = active != null && await _approvals.AllPartiesApprovedAsync(active.Id);

            var auditLogs = await _context.ContractAuditLogs
                .Where(l => l.EntityName == "ProjectContract" || l.EntityName == "ContractApproval")
                .OrderByDescending(l => l.CreatedDate)
                .Take(20)
                .ToListAsync();

            ViewData["ProjectId"]   = projectId;
            ViewData["UserRole"]    = role;
            ViewData["Versions"]    = versions;
            ViewData["Approvals"]   = approvals;
            ViewData["AuditLogs"]   = auditLogs;
            ViewData["AllApproved"] = allApproved;

            return View(active); // null = no contract yet
        }
    }
}
