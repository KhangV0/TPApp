using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TPApp.Data.Entities;
using TPApp.Entities;

namespace TPApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public List<int> UspSelectSubMenu(int menuId)
        {
            return SubMenuIds
                .FromSqlInterpolated($"EXEC uspSelectSubMenu {menuId}")
                .AsEnumerable()              
                .Select(x => x.MenuId)       
                .ToList();
        }

        public DbSet<SubMenuIdDto> SubMenuIds { get; set; }

        public DbSet<SanPhamCNTB> SanPhamCNTBs { get; set; }
        public DbSet<SanPhamCNTBCategory> SanPhamCNTBCategories { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<ContentsYeuCau> ContentsYeuCaus { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<KeywordEntity> KeywordEntities { get; set; }
        public DbSet<KeywordLienKet> KeywordLienKets { get; set; }
        public DbSet<NhaCungUng> NhaCungUngs { get; set; }
        public DbSet<VSImage> VSImages { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ForumYCTB> ForumYCTBs { get; set; }
        public DbSet<ForumYCDV> ForumYCDVs { get; set; }
        public DbSet<CommentsYCTB> CommentsYCTBs { get; set; }
        public DbSet<uspPortletCountTichcuu_Result> PortletHoiNhieu { get; set; }
        public DbSet<uspPortletCountTichcuuTraloi_Result> PortletTraLoiNhieu { get; set; }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<ProjectStep> ProjectSteps { get; set; }
        
        // Permission System
        public DbSet<StepPermission> StepPermissions { get; set; }
        public DbSet<ProjectConsultant> ProjectConsultants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SubMenuIdDto>().HasNoKey();

            modelBuilder.Entity<uspPortletCountTichcuu_Result>().HasNoKey();
            modelBuilder.Entity<uspPortletCountTichcuuTraloi_Result>().HasNoKey();

            // Configure Identity Mapping to existing User table
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");

                entity.Property(e => e.Id).HasColumnName("UserId");
                entity.Property(e => e.PasswordHash).HasColumnName("Password");
                
                // Map PhoneNumber to Mobile
                entity.Property(e => e.PhoneNumber).HasColumnName("Mobile");

                // IGNORE Identity columns that don't exist in existing table
                entity.Ignore(e => e.NormalizedUserName);
                entity.Ignore(e => e.NormalizedEmail);
                entity.Ignore(e => e.TwoFactorEnabled);
                entity.Ignore(e => e.EmailConfirmed);
                entity.Ignore(e => e.PhoneNumberConfirmed);
                entity.Ignore(e => e.AccessFailedCount);
                entity.Ignore(e => e.LockoutEnabled);
                entity.Ignore(e => e.LockoutEnd);
                entity.Ignore(e => e.SecurityStamp);
                entity.Ignore(e => e.ConcurrencyStamp);
            });

            // Ignore other Identity tables
            modelBuilder.Ignore<IdentityRole<int>>();
            modelBuilder.Ignore<IdentityUserToken<int>>();
            modelBuilder.Ignore<IdentityUserRole<int>>();
            modelBuilder.Ignore<IdentityUserLogin<int>>();
            modelBuilder.Ignore<IdentityUserClaim<int>>();
            modelBuilder.Ignore<IdentityRoleClaim<int>>();
        }

        public DbSet<Feedback> Feedbacks { get; set; }
        // Users DbSet is now provided by IdentityDbContext as Users property, but typed as ApplicationUser
        // We can expose it as Users if we want, or use the inherited property.
        // public DbSet<User> Users { get; set; } // REMOVED
        
        public DbSet<PhieuYeuCauCNTB> PhieuYeuCauCNTBs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<TimKiemDoiTac> TimKiemDoiTacs { get; set; }
        public DbSet<NhaTuVan> NhaTuVans { get; set; }
        public DbSet<ImageAdver> ImageAdvers { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<SearchIndexContent> SearchIndexContents { get; set; }
        public DbSet<SearchQueryLog> SearchQueryLogs { get; set; }
        public DbSet<Likepage> Likepages { get; set; }
        public DbSet<TechTransferRequest> TechTransferRequests { get; set; } = null!;
        public DbSet<NDAAgreement> NDAAgreements { get; set; } = null!;
        public DbSet<RFQRequest> RFQRequests { get; set; } = null!;
        public DbSet<ProposalSubmission> ProposalSubmissions { get; set; } = null!;
        public DbSet<ProposalScore> ProposalScores { get; set; } = null!;
        public DbSet<NegotiationForm> NegotiationForms { get; set; } = null!;
        public DbSet<LegalReviewForm> LegalReviewForms { get; set; } = null!;
        public DbSet<ContractComment> ContractComments { get; set; } = null!;

        // Step 6+7: Contract versioning & digital signing
        public DbSet<ProjectContract> ProjectContracts { get; set; } = null!;
        public DbSet<ContractApproval> ContractApprovals { get; set; } = null!;
        public DbSet<ContractSignatureRequest> ContractSignatureRequests { get; set; } = null!;
        public DbSet<ContractSignature> ContractSignatures { get; set; } = null!;
        public DbSet<ContractAuditLog> ContractAuditLogs { get; set; } = null!;

        // Profile verification
        public DbSet<UserOtp> UserOtps { get; set; } = null!;
        public DbSet<UserVerificationDoc> UserVerificationDocs { get; set; } = null!;

        public DbSet<EContract> EContracts { get; set; } = null!;
        public DbSet<AdvancePaymentConfirmation> AdvancePaymentConfirmations { get; set; } = null!;
        public DbSet<PilotTestReport> PilotTestReports { get; set; } = null!;
        public DbSet<ImplementationLog> ImplementationLogs { get; set; } = null!;
        public DbSet<HandoverReport> HandoverReports { get; set; } = null!;
        public DbSet<TrainingHandover> TrainingHandovers { get; set; } = null!;
        public DbSet<TechnicalDocHandover> TechnicalDocHandovers { get; set; } = null!;
        public DbSet<AcceptanceReport> AcceptanceReports { get; set; } = null!;
        public DbSet<LiquidationReport> LiquidationReports { get; set; } = null!;


        // State Machine Entities (Enterprise Workflow)
        public DbSet<ProjectWorkflowState> ProjectWorkflowStates { get; set; } = null!;
        public DbSet<ProjectStepState> ProjectStepStates { get; set; } = null!;
        public DbSet<WorkflowTransitionLog> WorkflowTransitionLogs { get; set; } = null!;

        // E-Sign System Entities
        public DbSet<TPApp.Entities.ESign.ESignDocument> ESignDocuments { get; set; } = null!;
        public DbSet<TPApp.Entities.ESign.ESignSignature> ESignSignatures { get; set; } = null!;
        public DbSet<TPApp.Entities.ESign.ESignAuditLog> ESignAuditLogs { get; set; } = null!;

        // AI Semantic Matching
        public DbSet<TPApp.Domain.Entities.SanPhamEmbedding> SanPhamEmbeddings { get; set; } = null!;
        public DbSet<TPApp.Domain.Entities.AISearchLog> AISearchLogs { get; set; } = null!;

        // RFQ Invitation System
        public DbSet<RFQInvitation> RFQInvitations { get; set; } = null!;
        public DbSet<ProjectAccessLog> ProjectAccessLogs { get; set; } = null!;

        // Notification System
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<SystemParameter> SystemParameters { get; set; } = null!;

        // CMS Lookup Tables
        public DbSet<CmsRole> CmsRoles { get; set; } = null!;
        public DbSet<CmsUserRole> CmsUserRoles { get; set; } = null!;
        public DbSet<RootSite> RootSites { get; set; } = null!;
        public DbSet<VAccountType> VAccountTypes { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<XuatXu> XuatXus { get; set; } = null!;
        public DbSet<MucDo> MucDos { get; set; } = null!;
    }
}
