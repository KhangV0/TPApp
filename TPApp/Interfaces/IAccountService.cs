using TPApp.ViewModel;
using TPApp.Entities;
using Microsoft.AspNetCore.Http;

namespace TPApp.Interfaces
{
    public interface IAccountService
    {
        Task<ProfileVm?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, ProfileVm model);
        Task<string?> UploadAvatarAsync(int userId, IFormFile file);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordVm model);
        Task<bool> SetPasswordAsync(int userId, string newPassword);
        Task SetPasswordHashBeforeInsert(ApplicationUser user, string password);
        Task<AccountSidebarVm?> GetSidebarDataAsync(int userId);
    }
}
