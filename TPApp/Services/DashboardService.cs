using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Enums;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    /// <summary>
    /// Service implementation for dashboard operations
    /// Provides optimized queries to fetch user dashboard data
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserDashboardVm> GetDashboardForUserAsync(string userId)
        {
            // Convert userId string to int (ApplicationUser.Id is int)
            if (!int.TryParse(userId, out int userIdInt))
            {
                throw new ArgumentException("Invalid user ID format", nameof(userId));
            }

            // Get user information
            var user = await _context.Users
                .Where(u => u.Id == userIdInt)
                .Select(u => new { u.FullName, u.UserName })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Get all projects where user is a member (optimized - single query)
            var userProjectIds = await _context.ProjectMembers
                .Where(pm => pm.UserId == userIdInt && pm.IsActive)
                .Select(pm => pm.ProjectId)
                .ToListAsync();

            if (!userProjectIds.Any())
            {
                // User has no projects
                return new UserDashboardVm
                {
                    FullName = user.FullName ?? user.UserName ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    TotalProjects = 0,
                    InProgressProjects = 0,
                    WaitingForMe = 0,
                    CompletedProjects = 0,
                    Projects = new List<ProjectDashboardItemVm>()
                };
            }

            // Get all projects data (optimized - single query)
            var projects = await _context.Projects
                .Where(p => userProjectIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.ProjectCode,
                    p.ProjectName
                })
                .ToListAsync();

            // Get all project members for role information (optimized - single query)
            var projectMembers = await _context.ProjectMembers
                .Where(pm => userProjectIds.Contains(pm.ProjectId) && pm.UserId == userIdInt)
                .Select(pm => new
                {
                    pm.ProjectId,
                    pm.Role
                })
                .ToListAsync();

            // Get all steps for these projects (optimized - single query to avoid N+1)
            var allSteps = await _context.ProjectSteps
                .Where(ps => userProjectIds.Contains(ps.ProjectId))
                .OrderBy(ps => ps.ProjectId)
                .ThenBy(ps => ps.StepNumber)
                .Select(ps => new StepDto
                {
                    ProjectId = ps.ProjectId,
                    StepNumber = ps.StepNumber,
                    StepName = ps.StepName,
                    StatusId = ps.StatusId
                })
                .ToListAsync();

            // Group steps by ProjectId for efficient lookup
            var stepsByProject = allSteps.GroupBy(s => s.ProjectId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Build project dashboard items
            var projectItems = new List<ProjectDashboardItemVm>();

            foreach (var project in projects)
            {
                var member = projectMembers.FirstOrDefault(pm => pm.ProjectId == project.Id);
                var steps = stepsByProject.ContainsKey(project.Id) 
                    ? stepsByProject[project.Id] 
                    : new List<StepDto>();

                // Determine current step
                // Logic: Find first InProgress step, or if none, check completion status
                var currentStep = steps.FirstOrDefault(s => s.StatusId == (int)StepStatus.InProgress);
                
                int currentStepNumber;
                string currentStepName;
                string currentStepStatus;

                if (currentStep != null)
                {
                    // There's an InProgress step
                    currentStepNumber = currentStep.StepNumber;
                    currentStepName = currentStep.StepName;
                    currentStepStatus = "InProgress";
                }
                else
                {
                    // No InProgress step - check if all completed or all not started
                    var allCompleted = steps.All(s => s.StatusId == (int)StepStatus.Completed);
                    var allNotStarted = steps.All(s => s.StatusId == (int)StepStatus.NotStarted);

                    if (allCompleted && steps.Any())
                    {
                        // Project completed - show last step
                        var lastStep = steps.OrderByDescending(s => s.StepNumber).First();
                        currentStepNumber = lastStep.StepNumber;
                        currentStepName = lastStep.StepName;
                        currentStepStatus = "Completed";
                    }
                    else if (allNotStarted || !steps.Any())
                    {
                        // Not started - show step 1 or default
                        var firstStep = steps.FirstOrDefault();
                        currentStepNumber = firstStep?.StepNumber ?? 1;
                        currentStepName = firstStep?.StepName ?? "Bước 1";
                        currentStepStatus = "NotStarted";
                    }
                    else
                    {
                        // Mixed status - find first NotStarted after last Completed
                        var lastCompleted = steps
                            .Where(s => s.StatusId == (int)StepStatus.Completed)
                            .OrderByDescending(s => s.StepNumber)
                            .FirstOrDefault();
                        
                        if (lastCompleted != null)
                        {
                            var nextStep = steps.FirstOrDefault(s => s.StepNumber > lastCompleted.StepNumber);
                            if (nextStep != null)
                            {
                                currentStepNumber = nextStep.StepNumber;
                                currentStepName = nextStep.StepName;
                                currentStepStatus = nextStep.StatusId == (int)StepStatus.Completed ? "Completed" : "NotStarted";
                            }
                            else
                            {
                                currentStepNumber = lastCompleted.StepNumber;
                                currentStepName = lastCompleted.StepName;
                                currentStepStatus = "Completed";
                            }
                        }
                        else
                        {
                            var firstStep = steps.First();
                            currentStepNumber = firstStep.StepNumber;
                            currentStepName = firstStep.StepName;
                            currentStepStatus = "NotStarted";
                        }
                    }
                }

                // Calculate progress
                int completedSteps = steps.Count(s => s.StatusId == (int)StepStatus.Completed);
                int progressPercent = steps.Any() ? (int)Math.Round((completedSteps / 11.0) * 100) : 0;

                // Build steps summary for visualization
                var stepsSummary = new List<StepMiniVm>();
                for (int i = 1; i <= 11; i++)
                {
                    var step = steps.FirstOrDefault(s => s.StepNumber == i);
                    stepsSummary.Add(new StepMiniVm
                    {
                        StepNumber = i,
                        StatusId = step?.StatusId ?? (int)StepStatus.NotStarted
                    });
                }

                // Get role name
                string roleName = member?.Role switch
                {
                    (int)ProjectRole.Buyer => "Buyer",
                    (int)ProjectRole.Seller => "Seller",
                    (int)ProjectRole.Consultant => "Consultant",
                    _ => "Unknown"
                };

                projectItems.Add(new ProjectDashboardItemVm
                {
                    ProjectId = project.Id,
                    Code = project.ProjectCode,
                    Name = project.ProjectName,
                    RoleName = roleName,
                    CurrentStepNumber = currentStepNumber,
                    CurrentStepName = currentStepName,
                    CurrentStepStatus = currentStepStatus,
                    CompletedSteps = completedSteps,
                    ProgressPercent = progressPercent,
                    StepsSummary = stepsSummary
                });
            }

            // Calculate statistics
            int totalProjects = projectItems.Count;
            
            // InProgress: projects with at least one InProgress step
            int inProgressProjects = projectItems.Count(p => 
                p.StepsSummary.Any(s => s.StatusId == (int)StepStatus.InProgress));
            
            // Completed: projects where step 11 is Completed OR all steps are Completed
            int completedProjects = projectItems.Count(p =>
            {
                var step11 = p.StepsSummary.FirstOrDefault(s => s.StepNumber == 11);
                return step11?.StatusId == (int)StepStatus.Completed ||
                       p.StepsSummary.All(s => s.StatusId == (int)StepStatus.Completed);
            });
            
            // WaitingForMe: Simplified logic - projects with InProgress steps where user is active member
            // (Can be enhanced later with step-to-role mapping)
            int waitingForMe = inProgressProjects;

            return new UserDashboardVm
            {
                FullName = user.FullName ?? user.UserName ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                TotalProjects = totalProjects,
                InProgressProjects = inProgressProjects,
                WaitingForMe = waitingForMe,
                CompletedProjects = completedProjects,
                Projects = projectItems
            };
        }
    }
}
