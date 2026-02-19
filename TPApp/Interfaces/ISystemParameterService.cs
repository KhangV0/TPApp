namespace TPApp.Interfaces
{
    public interface ISystemParameterService
    {
        /// <summary>Get parameter value by Name key. Returns null if not found.</summary>
        Task<string?> GetAsync(string key);

        /// <summary>Get parameter as int with fallback default.</summary>
        Task<int> GetIntAsync(string key, int defaultValue = 0);

        /// <summary>Get parameter as bool. Accepts: true/false, 1/0, yes/no.</summary>
        Task<bool> GetBoolAsync(string key, bool defaultValue = false);

        /// <summary>Get required parameter. Throws InvalidOperationException if missing or empty.</summary>
        Task<string> GetRequiredAsync(string key);
    }
}
