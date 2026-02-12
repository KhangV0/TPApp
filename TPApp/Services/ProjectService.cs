using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MyProjectVm>> GetMyProjectsAsync(int userId)
        {
            var projects = await _context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .Include(pm => pm.Project)
                .Select(pm => new
                {
                    pm.Project.Id,
                    pm.Project.ProjectCode,
                    pm.Project.ProjectName,
                    pm.Role,
                    pm.Project.StatusId,
                    pm.Project.CreatedDate
                })
                .ToListAsync();

            var result = new List<MyProjectVm>();

            foreach (var p in projects)
            {
                // Get completed steps count (StatusId == 2)
                var completedSteps = await _context.ProjectSteps
                    .Where(ps => ps.ProjectId == p.Id && ps.StatusId == 2)
                    .CountAsync();

                // Calculate progress (11 total steps)
                var progress = (int)Math.Round((completedSteps / 11.0) * 100);

                // Get current step (first incomplete step where StatusId != 2)
                var currentStep = await _context.ProjectSteps
                    .Where(ps => ps.ProjectId == p.Id && ps.StatusId != 2)
                    .OrderBy(ps => ps.StepNumber)
                    .Select(ps => ps.StepNumber)
                    .FirstOrDefaultAsync();

                if (currentStep == 0) currentStep = 11; // All completed

                result.Add(new MyProjectVm
                {
                    Id = p.Id,
                    Code = p.ProjectCode,
                    Name = p.ProjectName,
                    RoleId = p.Role,
                    Role = GetRoleName(p.Role),
                    CurrentStep = currentStep,
                    StatusId = p.StatusId,
                    Status = GetStatusName(p.StatusId),
                    ProgressPercent = progress,
                    CreatedDate = p.CreatedDate
                });
            }

            return result.OrderByDescending(p => p.CreatedDate).ToList();
        }

        public async Task<int> GetProjectCountAsync(int userId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .CountAsync();
        }

        private string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => "Buyer",
                2 => "Seller",
                3 => "Consultant",
                _ => "Unknown"
            };
        }

        private string GetStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Draft",
                2 => "Active",
                3 => "Completed",
                4 => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}
