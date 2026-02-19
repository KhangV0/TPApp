namespace TPApp.Configuration
{
    /// <summary>
    /// Centralized registry of all SYS_PARAMETERS keys.
    /// All keys follow convention: DOMAIN_FEATURE_DETAIL (uppercase, underscore-separated).
    /// </summary>
    public static class ParameterKeys
    {
        // ── SMTP ─────────────────────────────────────────────────────────────
        public const string SmtpProvider           = "SMTP_PROVIDER";
        public const string SmtpHost               = "SMTP_HOST";
        public const string SmtpPort               = "SMTP_PORT";
        public const string SmtpEnableSsl          = "SMTP_ENABLE_SSL";
        public const string SmtpEmail              = "SMTP_EMAIL";
        public const string SmtpPassword           = "SMTP_PASSWORD";          // sensitive
        public const string SmtpDisplayName        = "SMTP_DISPLAY_NAME";
        public const string SmtpMaxRetry           = "SMTP_MAX_RETRY";
        public const string SmtpRetryDelaySeconds  = "SMTP_RETRY_DELAY_SECONDS";

        // ── SMS ──────────────────────────────────────────────────────────────
        public const string SmsProvider            = "SMS_PROVIDER";
        public const string SmsAccountId           = "SMS_ACCOUNT_ID";         // sensitive
        public const string SmsAuthToken           = "SMS_AUTH_TOKEN";         // sensitive
        public const string SmsFromNumber          = "SMS_FROM_NUMBER";
        public const string SmsMaxRetry            = "SMS_MAX_RETRY";
        public const string SmsRetryDelaySeconds   = "SMS_RETRY_DELAY_SECONDS";

        // ── OTP ──────────────────────────────────────────────────────────────
        public const string OtpLength                  = "OTP_LENGTH";
        public const string OtpTtlSeconds              = "OTP_TTL_SECONDS";
        public const string OtpResendCooldownSeconds   = "OTP_RESEND_COOLDOWN_SECONDS";
        public const string OtpMaxAttemptVerify        = "OTP_MAX_ATTEMPT_VERIFY";

        // ── NOTIFICATION ─────────────────────────────────────────────────────
        public const string NotificationBatchSize       = "NOTIFICATION_BATCH_SIZE";
        public const string NotificationIntervalSeconds = "NOTIFICATION_PROCESS_INTERVAL_SECONDS";
        public const string NotificationEnableEmail     = "NOTIFICATION_ENABLE_EMAIL";
        public const string NotificationEnableSms       = "NOTIFICATION_ENABLE_SMS";
    }
}
