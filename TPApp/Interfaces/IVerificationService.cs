using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    public interface IVerificationService
    {
        Task<bool> SendEmailOtpAsync(int userId);
        Task<bool> SendPhoneOtpAsync(int userId);
        Task<(bool ok, string msg)> VerifyEmailOtpAsync(int userId, string otp);
        Task<(bool ok, string msg)> VerifyPhoneOtpAsync(int userId, string otp);
        Task<(bool ok, string msg)> UploadDocAsync(int userId, int docType, IFormFile file, IWebHostEnvironment env);
        Task<List<VerifyDocVm>> GetDocsAsync(int userId);
        Task<bool> UpdatePhoneAsync(int userId, string phone);
    }
}
