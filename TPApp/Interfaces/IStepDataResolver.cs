namespace TPApp.Interfaces
{
    /// <summary>
    /// Resolves step-specific data for display and editing
    /// Replaces the switch-case LoadStepData pattern
    /// </summary>
    public interface IStepDataResolver
    {
        /// <summary>
        /// Load data for a specific step
        /// Returns the entity object for that step (e.g., TechTransferRequest for step 1)
        /// </summary>
        Task<object?> LoadStepDataAsync(int projectId, int stepNumber);

        /// <summary>
        /// Save/update data for a specific step
        /// Returns true if successful, false otherwise
        /// </summary>
        Task<bool> SaveStepDataAsync(int projectId, int stepNumber, Dictionary<string, string> formData, int userId);

        /// <summary>
        /// Get the data table name for a specific step
        /// Used for DataRefTable in ProjectStepState
        /// </summary>
        string GetDataTableName(int stepNumber);

        /// <summary>
        /// Get the data record ID for a specific step
        /// Used for DataRefId in ProjectStepState
        /// </summary>
        Task<string?> GetDataRecordIdAsync(int projectId, int stepNumber);
    }
}
