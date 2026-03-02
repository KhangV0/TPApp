using TPApp.Entities;

namespace TPApp.Interfaces
{
    public interface IContractSigningService
    {
        /// <summary>Initiate OTP e-sign flow for Buyer.</summary>
        Task<(bool ok, string message, int requestId)> StartBuyerOtpAsync(int contractId, int userId, string? ipAddress);

        /// <summary>Verify OTP and record completed Buyer signature.</summary>
        Task<(bool ok, string message)> ConfirmBuyerOtpAsync(int requestId, int userId, string otpCode, string? ipAddress, string? userAgent);

        /// <summary>Initiate OTP e-sign flow for Seller.</summary>
        Task<(bool ok, string message, int requestId)> StartSellerOtpAsync(int contractId, int userId, string? ipAddress);

        /// <summary>Verify OTP and record completed Seller signature.</summary>
        Task<(bool ok, string message)> ConfirmSellerOtpAsync(int requestId, int userId, string otpCode, string? ipAddress, string? userAgent);

        /// <summary>Initiate CA remote signing for Seller via provider adapter.</summary>
        Task<(bool ok, string message, int requestId)> StartSellerCAAsync(int contractId, int userId, string provider, string callbackUrl, string? ipAddress);

        /// <summary>Handle webhook callback from CA provider. Validates secret + records signature.</summary>
        Task<bool> HandleProviderCallbackAsync(string provider, string requestRef, string callbackSecret, byte[]? signedPdfBytes, string? certSerial, string? certSubject, string? certIssuer, string? rawPayload);

        /// <summary>Get signing status for a contract (buyer + seller).</summary>
        Task<ContractSigningStatusDto> GetStatusAsync(int contractId);

        /// <summary>Check if both parties signed → complete Step 7 and unlock Step 8.</summary>
        Task<bool> TryCompleteStep7Async(int projectId, int contractId);
    }

    public class ContractSigningStatusDto
    {
        public bool BuyerSigned { get; set; }
        public DateTime? BuyerSignedAt { get; set; }
        public bool SellerSigned { get; set; }
        public DateTime? SellerSignedAt { get; set; }
        public bool FullySigned => BuyerSigned && SellerSigned;
    }
}
