using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    /// <summary>
    /// Service for managing proposal submissions with strict guards
    /// </summary>
    public class ProposalService : IProposalService
    {
        private readonly AppDbContext _context;
        private readonly IESignGateway _eSignGateway;
        private readonly IInvitationService _invitationService;

        public ProposalService(
            AppDbContext context, 
            IESignGateway eSignGateway,
            IInvitationService invitationService)
        {
            _context = context;
            _eSignGateway = eSignGateway;
            _invitationService = invitationService;
        }

        public async Task<bool> CanSubmitProposalAsync(int projectId, int sellerId)
        {
            // Guard 1: Must have valid invitation
            if (!await _invitationService.HasValidInvitationAsync(projectId, sellerId))
            {
                return false;
            }

            // Guard 2: Invitation must be Accepted
            var invitation = await _context.RFQInvitations
                .Include(i => i.RFQRequest)
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && 
                                         i.SellerId == sellerId && 
                                         i.IsActive);

            if (invitation == null || invitation.StatusId != (int)RFQInvitationStatus.Accepted)
            {
                return false;
            }

            // Guard 3: NDA must be signed
            var ndaSigned = await _eSignGateway.HasUserSignedProjectNda(projectId, sellerId);
            if (!ndaSigned)
            {
                return false;
            }


            // Guard 4: RFQ deadline not expired
            if (invitation.RFQRequest?.HanChotNopHoSo < DateTime.Now)
            {
                return false;
            }

            // NOTE: Removed Guard 5 (Step 4 InProgress check)
            // Seller can submit proposal after accepting invitation and signing NDA
            // Step 4 status will be updated AFTER proposal submission, not before
            
            // ORIGINAL Guard 5 (commented out):
            // var step4 = await _context.ProjectStepStates
            //     .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == 4);
            // if (step4 == null || step4.Status != 1) // 1 = InProgress
            // {
            //     return false;
            // }

            return true;
        }

        public async Task<ProposalSubmission> SubmitProposalAsync(int projectId, int sellerId, ProposalSubmissionDto dto)
        {
            if (!await CanSubmitProposalAsync(projectId, sellerId))
            {
                throw new InvalidOperationException("Cannot submit proposal. Check invitation status, NDA signature, and deadline.");
            }

            // Check if proposal already exists (update scenario)
            var existing = await _context.ProposalSubmissions
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.NguoiTao == sellerId);

            ProposalSubmission proposal;

            if (existing != null)
            {
                // Update existing proposal
                existing.GiaiPhapKyThuat = dto.GiaiPhapKyThuat;
                existing.BaoGiaSoBo = dto.BaoGiaSoBo;
                existing.ThoiGianTrienKhai = dto.ThoiGianTrienKhai;
                existing.HoSoNangLucDinhKem = dto.HoSoNangLucDinhKem;
                existing.StatusId = (int)ProposalStatus.Submitted;
                existing.SubmittedDate = DateTime.Now;
                existing.NguoiSua = sellerId;
                existing.NgaySua = DateTime.Now;

                proposal = existing;
            }
            else
            {
                // Create new proposal
                proposal = new ProposalSubmission
                {
                    ProjectId = projectId,
                    GiaiPhapKyThuat = dto.GiaiPhapKyThuat,
                    BaoGiaSoBo = dto.BaoGiaSoBo,
                    ThoiGianTrienKhai = dto.ThoiGianTrienKhai,
                    HoSoNangLucDinhKem = dto.HoSoNangLucDinhKem,
                    StatusId = (int)ProposalStatus.Submitted,
                    SubmittedDate = DateTime.Now,
                    NguoiTao = sellerId,
                    NgayTao = DateTime.Now
                };

                _context.ProposalSubmissions.Add(proposal);
            }

            // Update invitation status to ProposalSubmitted
            var invitation = await _context.RFQInvitations
                .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.SellerId == sellerId && i.IsActive);

            if (invitation != null)
            {
                invitation.StatusId = (int)RFQInvitationStatus.ProposalSubmitted;
            }

            await _context.SaveChangesAsync();

            // IMPORTANT: Do NOT set Project.SelectedSellerId here
            // Seller is only assigned when buyer selects the proposal

            return proposal;
        }

        public async Task<List<ProposalSubmission>> GetProjectProposalsAsync(int projectId)
        {
            return await _context.ProposalSubmissions
                .Where(p => p.ProjectId == projectId && p.StatusId == (int)ProposalStatus.Submitted)
                .OrderByDescending(p => p.SubmittedDate)
                .ToListAsync();
        }

        public async Task<ProposalSubmission?> GetSellerProposalAsync(int projectId, int sellerId)
        {
            return await _context.ProposalSubmissions
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.NguoiTao == sellerId);
        }

        public async Task<bool> HasSubmittedProposalAsync(int projectId, int sellerId)
        {
            return await _context.ProposalSubmissions
                .AnyAsync(p => p.ProjectId == projectId && 
                              p.NguoiTao == sellerId && 
                              p.StatusId == (int)ProposalStatus.Submitted);
        }
    }
}
