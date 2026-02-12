using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(IProjectService projectService, UserManager<ApplicationUser> userManager)
        {
            _projectService = projectService;
            _userManager = userManager;
        }

        // GET: /Projects/MyProjects
        [HttpGet]
        public async Task<IActionResult> MyProjects()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var projects = await _projectService.GetMyProjectsAsync(userId);
            
            return View(projects);
        }
    }
}
