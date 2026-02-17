using TPApp.Entities.ESign;

namespace TPApp.Interfaces
{
    /// <summary>
    /// Gateway for E-Sign system operations
    /// Handles document signing, OTP verification, and signature validation
    /// </summary>
    public interface IESignGateway
    {
        /// <summary>
        /// Check if the project NDA has been signed by the buyer
        /// This is the guard condition for Step 2 completion
        /// </summary>
        Task<bool> IsProjectNdaSignedAsync(int projectId, int buyerUserId);

        /// <summary>
        /// Get the project NDA document
        /// </summary>
        Task<ESignDocument?> GetProjectNdaAsync(int projectId);

        /// <summary>
        /// Create a new E-Sign document
        /// </summary>
        Task<ESignDocument> CreateDocumentAsync(int projectId, int docType, string documentName, int createdBy);

        /// <summary>
        /// Upload a document file and calculate hash
        /// </summary>
        Task<string> UploadDocumentAsync(long documentId, Stream fileStream, string fileName);

        /// <summary>
        /// Send OTP code to user's phone
        /// Returns the OTP code (for testing) or null in production
        /// </summary>
        Task<string?> SendOtpAsync(int userId, string phoneNumber);

        /// <summary>
        /// Verify OTP code entered by user
        /// </summary>
        Task<bool> VerifyOtpAsync(long signatureId, string otpCode);

        /// <summary>
        /// Sign a document after OTP verification
        /// </summary>
        Task SignDocumentAsync(long documentId, int userId, string signerRole, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Get all signatures for a document
        /// </summary>
        Task<List<ESignSignature>> GetDocumentSignaturesAsync(long documentId);

        /// <summary>
        /// Log an E-Sign action for audit trail
        /// </summary>
        Task LogActionAsync(long documentId, int userId, string action, string? details = null, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Check if a specific user has signed the project NDA
        /// Used for seller authorization checks
        /// </summary>
        Task<bool> HasUserSignedProjectNda(int projectId, int userId);
    }
}
