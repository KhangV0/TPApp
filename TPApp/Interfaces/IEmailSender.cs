namespace TPApp.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body, bool isHtml = true);
    }
}
