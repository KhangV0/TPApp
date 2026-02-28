namespace TPApp.ViewModel
{
    public class FeedbackCreateViewModel
    {
        public string? FullName    { get; set; }
        public string? Email       { get; set; }
        public string? Address     { get; set; }
        public string? Phone       { get; set; }
        public string? Content     { get; set; }
        public string? Title       { get; set; }
        public string? Description { get; set; }

        // ── Math Captcha ──────────────────────────────────────
        /// <summary>Câu hỏi hiển thị, VD: "7 + 4"</summary>
        public string CaptchaQuestion { get; set; } = "";

        /// <summary>Đáp án user nhập — được validate server-side.</summary>
        public string? CaptchaAnswer  { get; set; }
    }
}
