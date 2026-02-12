using TPApp.ViewModel;
using Microsoft.AspNetCore.Http;

namespace TPApp.Interfaces
{
    public interface IAccountService
    {
        Task<ProfileVm?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, ProfileVm model);
        Task<string?> UploadAvatarAsync(int userId, IFormFile file);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordVm model);
        Task<AccountSidebarVm?> GetSidebarDataAsync(int userId);
    }
}
