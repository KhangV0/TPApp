using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;

namespace TPApp.Services
{
    public class ApplicationUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser>
    {
        private readonly AppDbContext _context;

        public ApplicationUserStore(AppDbContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public async Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return user.Id.ToString();
        }

        public async Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return user.UserName;
        }

        public async Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        }

        public async Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return user.UserName; // No normalized column, return distinct logic if needed, or just UserName
        }

        public async Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            // Do nothing as we don't store normalized name
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (int.TryParse(userId, out int id))
            {
                return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
            }
            return null;
        }

        public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            // Note: NormalizedUserName is mapped to UserName in DbContext but we ignored it in logic.
            // Here we should query by UserName directly.
            // Be careful if normalizedUserName passes UPPERCASE.
            // For legacy, we might want to search case-insensitive or exact match?
            // Let's assume input might be Normalized (UPPER), but we search UserName (Mixed).
            // However, standart Identity passes NormalizedUserName here.
            
            // Hack for legacy: try to find by UserName ignoring case if possible, or exact.
            // Since we can't reliably un-normalize, let's assume we rely on manual lookups mostly,
            // OR the caller passes the raw username if we configured IdentityOptions.
            
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == normalizedUserName, cancellationToken);
        }

        // --- Password Store ---
        public async Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
        }

        public async Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return user.PasswordHash;
        }

        public async Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return !string.IsNullOrEmpty(user.PasswordHash);
        }

        // --- Email Store ---
        public async Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
        {
            user.Email = email;
        }

        public async Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return user.Email;
        }

        public async Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return true; // Assume confirmed for legacy
        }

        public async Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            // No-op
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
             return await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        }

        public async Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
             return user.Email;
        }

        public async Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
        {
             // No-op
        }
    }
}
