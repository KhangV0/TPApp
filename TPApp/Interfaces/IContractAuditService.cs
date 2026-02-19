namespace TPApp.Interfaces
{
    public interface IContractAuditService
    {
        Task AppendAsync(string entityName, string entityId, string action,
                         object? data = null, int? actorUserId = null, string? ipAddress = null);
    }
}
