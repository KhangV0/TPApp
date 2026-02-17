using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Services
{
    public interface IWorkflowService
    {
        Task InitializeProjectSteps(int projectId);
        Task CompleteStep(int projectId, int stepNumber);
        Task<bool> CanAccessStep(int projectId, int stepNumber);
        Task<List<ProjectStep>> GetProjectSteps(int projectId);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly AppDbContext _context;

        public WorkflowService(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitializeProjectSteps(int projectId)
        {
            var steps = new List<ProjectStep>
            {
                new ProjectStep { ProjectId = projectId, StepNumber = 1, StepName = "Yêu cầu chuyển giao công nghệ", StatusId = 1 }, // InProgress
                new ProjectStep { ProjectId = projectId, StepNumber = 2, StepName = "Thỏa thuận bảo mật (NDA)", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 3, StepName = "Yêu cầu báo giá (RFQ)", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 4, StepName = "Nộp hồ sơ đề xuất", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 5, StepName = "Đàm phán thương mại", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 6, StepName = "Kiểm tra pháp lý", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 7, StepName = "Ký hợp đồng điện tử", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 8, StepName = "Xác nhận tạm ứng", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 9, StepName = "Thử nghiệm Pilot", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 10, StepName = "Bàn giao & triển khai thiết bị", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 11, StepName = "Đào tạo & chuyển giao vận hành", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 12, StepName = "Bàn giao hồ sơ kỹ thuật", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 13, StepName = "Nghiệm thu", StatusId = 0 },
                new ProjectStep { ProjectId = projectId, StepNumber = 14, StepName = "Thanh lý hợp đồng", StatusId = 0 }
            };

            await _context.ProjectSteps.AddRangeAsync(steps);
            await _context.SaveChangesAsync();
        }

        public async Task CompleteStep(int projectId, int stepNumber)
        {
            var currentStep = await _context.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == stepNumber);

            if (currentStep != null && currentStep.StatusId != 2)
            {
                currentStep.StatusId = 2; // Completed
                currentStep.CompletedDate = DateTime.Now;

                // Unlock next step
                var nextStep = await _context.ProjectSteps
                    .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == stepNumber + 1);
                
                if (nextStep != null)
                {
                    nextStep.StatusId = 1; // InProgress
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CanAccessStep(int projectId, int stepNumber)
        {
            // Step 1 check logic (always allowed if project exists? or implicit)
            if (stepNumber == 1) return true;

            var step = await _context.ProjectSteps
                .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.StepNumber == stepNumber);

            // Access allowed if NotStarted (0) is FALSE. 
            // Only InProgress (1) or Completed (2) allow access (to view/edit).
            // Actually, requirements say: "Nếu StatusId = NotStarted -> return false"
            
            if (step == null) return false;
            return step.StatusId != 0;
        }

        public async Task<List<ProjectStep>> GetProjectSteps(int projectId)
        {
            return await _context.ProjectSteps
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.StepNumber)
                .ToListAsync();
        }
    }
}
