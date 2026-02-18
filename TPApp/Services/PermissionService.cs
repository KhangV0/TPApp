using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Enums;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    /// <summary>
    /// Implementation of permission service with role-based access control and dynamic guards
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly IESignGateway _eSignGateway;

        public PermissionService(AppDbContext context, IESignGateway eSignGateway)
        {
            _context = context;
            _eSignGateway = eSignGateway;
        }

        public async Task<StepPermissionResult> GetStepPermissionsAsync(int projectId, int stepNumber, int userId)
        {
            // 1. Get user role for this project
            var roleType = await GetUserRoleAsync(userId, projectId);

            // 2. Load base permissions from matrix
            var basePermission = await _context.StepPermissions
                .FirstOrDefaultAsync(p => p.StepNumber == stepNumber && p.RoleType == roleType);

            if (basePermission == null)
            {
                return StepPermissionResult.Blocked($"No permissions defined for step {stepNumber} and role {roleType}");
            }

            var result = new StepPermissionResult
            {
                CanView = basePermission.CanView,
                CanEdit = basePermission.CanEdit,
                CanSubmit = basePermission.CanSubmit,
                CanApprove = basePermission.CanApprove
            };

            // 3. Apply dynamic guards
            await ApplyDynamicGuardsAsync(result, projectId, stepNumber, userId, roleType);

            return result;
        }

        public async Task EnsureCanAsync(int projectId, int stepNumber, int userId, PermissionAction action)
        {
            var permissions = await GetStepPermissionsAsync(projectId, stepNumber, userId);

            if (permissions.IsBlocked)
            {
                throw new UnauthorizedAccessException(permissions.BlockedReason);
            }

            var hasPermission = action switch
            {
                PermissionAction.View => permissions.CanView,
                PermissionAction.Edit => permissions.CanEdit,
                PermissionAction.Submit => permissions.CanSubmit,
                PermissionAction.Approve => permissions.CanApprove,
                _ => false
            };

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException($"You do not have permission to {action} on step {stepNumber}");
            }
        }

        public async Task<UserRoleType> GetUserRoleAsync(int userId, int projectId)
        {
            // Check if user is Admin (you may need to add IsAdmin field to Users table or check a role)
            var user = await _context.Users.FindAsync(userId);
            if (user?.IsAdmin == true) // Assuming IsAdmin field exists
            {
                return UserRoleType.Admin;
            }

            // Check if user is Consultant assigned to this project
            var isConsultant = await _context.ProjectConsultants
                .AnyAsync(pc => pc.ProjectId == projectId && pc.ConsultantId == userId && pc.IsActive);
            if (isConsultant)
            {
                return UserRoleType.Consultant;
            }

            // Check if user is Seller (has NhaCungUng record)
            var isSeller = await _context.NhaCungUngs.AnyAsync(n => n.UserId == userId);
            if (isSeller)
            {
                return UserRoleType.Seller;
            }

            // Default to Buyer (project creator/member)
            return UserRoleType.Buyer;
        }

        public async Task<bool> HasSignedNdaAsync(int projectId, int userId, UserRoleType roleType)
        {
            // Check if NDA is signed using E-Sign gateway
            var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(projectId, userId);
            return ndaSigned;
        }

        #region Private Helper Methods

        private async Task ApplyDynamicGuardsAsync(
            StepPermissionResult result,
            int projectId,
            int stepNumber,
            int userId,
            UserRoleType roleType)
        {
            // Guard 1: Consultant must be assigned to project
            if (roleType == UserRoleType.Consultant)
            {
                var isAssigned = await _context.ProjectConsultants
                    .AnyAsync(pc => pc.ProjectId == projectId && pc.ConsultantId == userId && pc.IsActive);

                if (!isAssigned)
                {
                    result.CanView = false;
                    result.CanEdit = false;
                    result.CanSubmit = false;
                    result.CanApprove = false;
                    result.BlockedReason = "Bạn chưa được phân công làm tư vấn cho dự án này";
                    return;
                }
            }

            // Guard 2: Seller must have RFQ invitation for seller-facing steps
            if (roleType == UserRoleType.Seller && stepNumber >= 2)
            {
                var hasInvitation = await _context.RFQInvitations
                    .AnyAsync(i => i.SellerId == userId &&
                                  i.ProjectId == projectId &&
                                  i.IsActive);

                if (!hasInvitation)
                {
                    result.CanView = false;
                    result.CanEdit = false;
                    result.CanSubmit = false;
                    result.CanApprove = false;
                    result.BlockedReason = "Bạn chưa được mời tham gia dự án này";
                    return;
                }

                // Guard 3: Seller must accept invitation before submitting proposal (Step 4)
                if (stepNumber == 4 && result.CanSubmit)
                {
                    var invitation = await _context.RFQInvitations
                        .FirstOrDefaultAsync(i => i.SellerId == userId &&
                                                 i.ProjectId == projectId &&
                                                 i.IsActive);

                    if (invitation == null || invitation.StatusId != (int)RFQInvitationStatus.Accepted)
                    {
                        result.CanSubmit = false;
                        result.BlockedReason = "Bạn phải chấp nhận lời mời trước khi gửi báo giá";
                        return;
                    }
                }
            }

            // Guard 4: NDA must be signed before accessing Step 3+ (Buyer)
            if (roleType == UserRoleType.Buyer && stepNumber >= 3)
            {
                var ndaSigned = await HasSignedNdaAsync(projectId, userId, roleType);
                if (!ndaSigned)
                {
                    result.CanView = false;
                    result.CanEdit = false;
                    result.CanSubmit = false;
                    result.CanApprove = false;
                    result.BlockedReason = "Bạn phải ký NDA (Bước 2) trước khi tiếp tục";
                    return;
                }
            }

            // Guard 5: NDA must be signed before viewing RFQ details/submitting proposal (Seller)
            if (roleType == UserRoleType.Seller && stepNumber >= 3)
            {
                var ndaSigned = await HasSignedNdaAsync(projectId, userId, roleType);
                if (!ndaSigned)
                {
                    // Seller can view invitation but not details
                    if (stepNumber == 4 && result.CanSubmit)
                    {
                        result.CanSubmit = false;
                        result.BlockedReason = "Bạn phải ký NDA trước khi gửi báo giá";
                        return;
                    }
                }
            }

            // Guard 6: Check if deadline has passed for proposal submission
            if (roleType == UserRoleType.Seller && stepNumber == 4 && result.CanSubmit)
            {
                var rfq = await _context.RFQRequests
                    .Where(r => r.ProjectId == projectId)
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefaultAsync();

                if (rfq != null && rfq.HanChotNopHoSo < DateTime.Now)
                {
                    result.CanSubmit = false;
                    result.BlockedReason = "Đã hết hạn nộp hồ sơ báo giá";
                    return;
                }
            }
        }

        #endregion
    }
}
