namespace TPApp.Configuration
{
    /// <summary>
    /// Configuration for OTP (One-Time Password) behaviour.
    /// Bind from appsettings.json section "OtpSettings".
    ///
    /// Example:
    /// "OtpSettings": {
    ///   "EmailOtpExpiryMinutes": 5,
    ///   "PhoneOtpExpiryMinutes": 5,
    ///   "NegotiationOtpExpirySeconds": 300
    /// }
    /// </summary>
    public class OtpSettings
    {
        public const string SectionName = "OtpSettings";

        /// <summary>How long (minutes) an email-verification OTP is valid. Default: 5.</summary>
        public int EmailOtpExpiryMinutes { get; set; } = 5;

        /// <summary>How long (minutes) a phone-verification OTP is valid. Default: 5.</summary>
        public int PhoneOtpExpiryMinutes { get; set; } = 5;

        /// <summary>How long (seconds) a negotiation/contract OTP is valid. Default: 300 (5 min).</summary>
        public int NegotiationOtpExpirySeconds { get; set; } = 300;

        /// <summary>
        /// Cooldown between consecutive OTP send requests (minutes).
        /// User must wait this long before requesting a new OTP.
        /// Default: 2 minutes.
        /// </summary>
        public int ResendCooldownMinutes { get; set; } = 2;

        // Derived helpers
        public TimeSpan EmailOtpExpiry       => TimeSpan.FromMinutes(EmailOtpExpiryMinutes);
        public TimeSpan PhoneOtpExpiry       => TimeSpan.FromMinutes(PhoneOtpExpiryMinutes);
        public TimeSpan NegotiationOtpExpiry => TimeSpan.FromSeconds(NegotiationOtpExpirySeconds);
    }
}
