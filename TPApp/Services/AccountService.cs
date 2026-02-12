using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Helpers;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IWebHostEnvironment _environment;
        private readonly IProjectService _projectService;

        public AccountService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IWebHostEnvironment environment,
            IProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _environment = environment;
            _projectService = projectService;
        }

        public async Task<ProfileVm?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return new ProfileVm
            {
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Email = user.Email ?? "",
                AvatarUrl = string.IsNullOrEmpty(user.Img) ? "/images/default-avatar.png" : user.Img,
                LastLogin = user.LastLogin,
                Created = user.Created
            };
        }

        public async Task<bool> UpdateProfileAsync(int userId, ProfileVm model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.NormalizedEmail = model.Email.ToUpperInvariant();

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> UploadAvatarAsync(int userId, IFormFile file)
        {
            // Validate image
            if (!ImageHelper.ValidateImage(file, out string errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            // Delete old avatar
            ImageHelper.DeleteOldAvatar(user.Img, _environment);

            // Create upload directory
            var uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            // Resize and save
            await ImageHelper.ResizeAndSaveImageAsync(file, filePath, 300, 300);

            // Update user record
            var relativePath = $"/uploads/avatars/{fileName}";
            user.Img = relativePath;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return relativePath;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordVm model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Verify current password
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? "", model.CurrentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidOperationException("Mật khẩu hiện tại không đúng");
            }

            // Hash new password
            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AccountSidebarVm?> GetSidebarDataAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var projectCount = await _projectService.GetProjectCountAsync(userId);

            return new AccountSidebarVm
            {
                FullName = user.FullName ?? user.UserName ?? "User",
                Email = user.Email ?? "",
                AvatarUrl = string.IsNullOrEmpty(user.Img) ? "/images/default-avatar.png" : user.Img,
                ProjectCount = projectCount
            };
        }
    }
}
