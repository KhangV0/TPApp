using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class ContractAuditService : IContractAuditService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ContractAuditService> _logger;

        public ContractAuditService(AppDbContext context, ILogger<ContractAuditService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task AppendAsync(string entityName, string entityId, string action,
                                      object? data = null, int? actorUserId = null, string? ipAddress = null)
        {
            try
            {
                var log = new ContractAuditLog
                {
                    EntityName  = entityName,
                    EntityId    = entityId,
                    Action      = action,
                    DataJson    = data != null ? JsonSerializer.Serialize(data) : null,
                    ActorUserId = actorUserId,
                    IPAddress   = ipAddress,
                    CreatedDate = DateTime.UtcNow
                };
                _context.ContractAuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to append audit log: {Entity}/{Id}/{Action}", entityName, entityId, action);
            }
        }
    }
}
