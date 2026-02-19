namespace TPApp.Interfaces
{
    /// <summary>Adapter interface for remote CA signing providers (VNPT, FPT, Viettel).</summary>
    public interface ISigningProvider
    {
        string ProviderName { get; }

        /// <summary>Submit PDF to provider for signing. Returns RequestRef (transaction id).</summary>
        Task<string> CreateSigningRequestAsync(byte[] pdfBytes, SignerInfo signer, string callbackUrl);

        /// <summary>Retrieve the signed PDF bytes and certificate information.</summary>
        Task<SignedResult?> GetSignedDocumentAsync(string requestRef);

        /// <summary>Verify a signed PDF document. Returns verification details.</summary>
        Task<VerificationResult> VerifySignedDocumentAsync(byte[] signedPdfBytes);
    }

    public class SignerInfo
    {
        public string FullName { get; set; } = "";
        public string Email    { get; set; } = "";
        public string Phone    { get; set; } = "";
        public string Title    { get; set; } = "";
    }

    public class SignedResult
    {
        public byte[]  SignedPdfBytes     { get; set; } = Array.Empty<byte>();
        public string? CertificateSerial  { get; set; }
        public string? CertificateSubject { get; set; }
        public string? CertificateIssuer  { get; set; }
        public string? TimeStampToken     { get; set; }
        public string? SignedHash         { get; set; }
        public string? RawPayload         { get; set; }
    }

    public class VerificationResult
    {
        public bool    IsValid { get; set; }
        public string? Details { get; set; }
        public int     Status  { get; set; } // 1=Valid, 2=Invalid
    }
}
