using System.Security.Claims;

namespace TPApp.Interfaces
{
    public interface ICmsAccessService
    {
        Task<bool> CanAccessCmsAsync(ClaimsPrincipal user);
    }
}
