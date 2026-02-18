namespace TPApp.Interfaces
{
    public interface IOtpEmailService
    {
        /// <summary>Send OTP code to user email for negotiation signing.</summary>
        Task SendOtpAsync(string toEmail, string fullName, string otp, string role, int projectId);
    }
}
