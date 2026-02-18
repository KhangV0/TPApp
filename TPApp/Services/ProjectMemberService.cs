using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Enums;
using TPApp.Interfaces;
using TPApp.ViewModel;

namespace TPApp.Services
{
    /// <summary>
    /// Service for managing project members
    /// </summary>
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly AppDbContext _context;

        public ProjectMemberService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectMemberDto>> GetMembersAsync(int projectId)
        {
            var members = await _context.ProjectMembers
                .Include(m => m.Project)
                .Where(m => m.ProjectId == projectId && m.IsActive)
                .ToListAsync();

            var memberDtos = new List<ProjectMemberDto>();

            foreach (var member in members)
            {
                var user = await _context.Users.FindAsync(member.UserId);
                if (user != null)
                {
                    memberDtos.Add(new ProjectMemberDto
                    {
                        Id = member.Id,
                        UserId = member.UserId,
                        UserName = user.FullName ?? user.UserName ?? "Unknown",
                        Email = user.Email ?? "",
                        Role = member.Role,
                        RoleName = GetRoleName(member.Role),
                        JoinedDate = member.JoinedDate,
                        IsActive = member.IsActive
                    });
                }
            }

            return memberDtos.OrderBy(m => m.Role).ThenBy(m => m.UserName).ToList();
        }

        public async Task AddConsultantAsync(int projectId, int userId, int currentUserId)
        {
            // Guard 1: Only buyer can add
            if (!await IsBuyerAsync(projectId, currentUserId))
            {
                throw new InvalidOperationException("Only project buyer can add consultants.");
            }

            // Guard 2: Cannot add duplicate
            var existing = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive);

            if (existing != null)
            {
                throw new InvalidOperationException("User is already a member of this project.");
            }

            // Add consultant
            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                Role = (int)ProjectRole.Consultant,
                JoinedDate = DateTime.Now,
                IsActive = true
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(int projectId, int userId, int currentUserId)
        {
            // Guard 1: Only buyer can remove
            if (!await IsBuyerAsync(projectId, currentUserId))
            {
                throw new InvalidOperationException("Only project buyer can remove members.");
            }

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive);

            if (member == null)
            {
                throw new InvalidOperationException("Member not found.");
            }

            // Guard 2: Cannot remove buyer
            if (member.Role == (int)ProjectRole.Buyer)
            {
                throw new InvalidOperationException("Cannot remove project buyer.");
            }

            // Soft delete
            member.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsBuyerAsync(int projectId, int currentUserId)
        {
            return await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId &&
                              m.UserId == currentUserId &&
                              m.Role == (int)ProjectRole.Buyer &&
                              m.IsActive);
        }

        public async Task<List<UserSelectDto>> GetAvailableConsultantsAsync(int projectId)
        {
            // Get all user IDs already in the project
            var existingMemberIds = await _context.ProjectMembers
                .Where(m => m.ProjectId == projectId && m.IsActive)
                .Select(m => m.UserId)
                .ToListAsync();

            // Get all users not in the project
            var availableUsers = await _context.Users
                .Where(u => !existingMemberIds.Contains(u.Id))
                .Select(u => new UserSelectDto
                {
                    UserId = u.Id,
                    FullName = u.FullName ?? u.UserName ?? "Unknown",
                    Email = u.Email ?? ""
                })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return availableUsers;
        }

        private string GetRoleName(int role)
        {
            return role switch
            {
                1 => "Buyer",
                2 => "Seller",
                3 => "Consultant",
                _ => "Unknown"
            };
        }
    }
}
