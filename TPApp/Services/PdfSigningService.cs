using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Canvas;
using SysPath = System.IO.Path;

namespace TPApp.Services;

/// <summary>
/// Embeds a visible digital signature block into a PDF using iText7.
/// The signature appearance is added at the bottom-right of the last page.
/// </summary>
public class PdfSigningService
{
    private readonly ILogger<PdfSigningService> _log;
    private readonly IWebHostEnvironment _env;

    public PdfSigningService(ILogger<PdfSigningService> log, IWebHostEnvironment env)
    {
        _log  = log;
        _env  = env;
    }

    /// <summary>
    /// Embeds a visible "Signed by" block into a PDF.
    /// Returns the path of the newly created signed PDF file.
    /// </summary>
    public async Task<string> EmbedVisibleSignatureAsync(
        string   sourcePdfPath,
        byte[]   signatureBytes,
        byte[]   certificateBytes,
        string   certSubject,
        string   certIssuer,
        string   certSerial,
        int      role,           // 1 = Buyer, 2 = Seller
        int      projectId)
    {
        _log.LogInformation("Embedding visible signature into PDF: {Path}", sourcePdfPath);

        // ─── Output path ───
        var dir = SysPath.Combine(_env.WebRootPath, "uploads", "contracts", $"proj_{projectId}", "signed");
        Directory.CreateDirectory(dir);
        var fileName   = $"signed_{(role == 1 ? "buyer" : "seller")}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var outputPath = SysPath.Combine(dir, fileName);

        // ─── Parse CN from subject ───
        var ownerName  = ParseCN(certSubject)  ?? certSubject;
        var issuerName = ParseCN(certIssuer)   ?? certIssuer;
        var signedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var roleLabel  = role == 1 ? "Bên mua" : "Bên bán";

        // ─── Read source PDF bytes ───
        var pdfBytes = await File.ReadAllBytesAsync(sourcePdfPath);

        using var srcMs  = new MemoryStream(pdfBytes);
        using var dstMs  = new MemoryStream();

        // leaveOpen:true → PdfWriter will NOT close dstMs on dispose
        using (var reader = new PdfReader(srcMs))
        {
            using var writer = new PdfWriter(dstMs, new WriterProperties());
            writer.SetCloseStream(false);
            using var pdfDoc = new PdfDocument(reader, writer);

            // Get last page
            var lastPage   = pdfDoc.GetLastPage();
            var pageSize   = lastPage.GetPageSize();

            // ─── Signature block dimensions (bottom-right corner) ───
            float blockW = 240f;
            float blockH = 80f;
            float margin  = 20f;
            float x = pageSize.GetRight() - blockW - margin;
            float y = pageSize.GetBottom() + margin;

            // ─── Draw using canvas ───
            var canvas = new PdfCanvas(lastPage);

            // Background box
            canvas.SetFillColor(new DeviceRgb(0.96f, 0.98f, 1.0f));  // light blue-grey
            canvas.Rectangle(x, y, blockW, blockH);
            canvas.Fill();

            // Border
            canvas.SetStrokeColor(new DeviceRgb(0.2f, 0.4f, 0.8f));  // blue
            canvas.SetLineWidth(1.5f);
            canvas.Rectangle(x, y, blockW, blockH);
            canvas.Stroke();

            // Blue header strip
            canvas.SetFillColor(new DeviceRgb(0.2f, 0.4f, 0.8f));
            canvas.Rectangle(x, y + blockH - 18f, blockW, 18f);
            canvas.Fill();

            // ─── Text content ───
            var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var fontBold    = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Header text
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 9f)
                  .SetFillColor(ColorConstants.WHITE)
                  .MoveText(x + 8f, y + blockH - 13f)
                  .ShowText($"\u2714 Ký số điện tử — {roleLabel}")
                  .EndText();

            float lineY = y + blockH - 28f;
            float lineH = 12f;

            // Signed by
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 7.5f)
                  .SetFillColor(new DeviceRgb(0.1f, 0.1f, 0.1f))
                  .MoveText(x + 8f, lineY)
                  .ShowText("Ký bởi: ")
                  .EndText();
            canvas.BeginText()
                  .SetFontAndSize(fontRegular, 7.5f)
                  .SetFillColor(new DeviceRgb(0.1f, 0.1f, 0.1f))
                  .MoveText(x + 42f, lineY)
                  .ShowText(Truncate(ownerName, 30))
                  .EndText();

            lineY -= lineH;

            // Issuer
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 7f)
                  .SetFillColor(new DeviceRgb(0.3f, 0.3f, 0.3f))
                  .MoveText(x + 8f, lineY)
                  .ShowText("Cấp bởi: ")
                  .EndText();
            canvas.BeginText()
                  .SetFontAndSize(fontRegular, 7f)
                  .SetFillColor(new DeviceRgb(0.3f, 0.3f, 0.3f))
                  .MoveText(x + 40f, lineY)
                  .ShowText(Truncate(issuerName, 28))
                  .EndText();

            lineY -= lineH;

            // Serial
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 6.5f)
                  .SetFillColor(new DeviceRgb(0.4f, 0.4f, 0.4f))
                  .MoveText(x + 8f, lineY)
                  .ShowText("Serial: ")
                  .EndText();
            canvas.BeginText()
                  .SetFontAndSize(fontRegular, 6.5f)
                  .SetFillColor(new DeviceRgb(0.4f, 0.4f, 0.4f))
                  .MoveText(x + 35f, lineY)
                  .ShowText(Truncate(certSerial, 36))
                  .EndText();

            lineY -= lineH;

            // Date
            canvas.BeginText()
                  .SetFontAndSize(fontBold, 7f)
                  .SetFillColor(new DeviceRgb(0.2f, 0.4f, 0.8f))
                  .MoveText(x + 8f, lineY)
                  .ShowText("Ngày ký: ")
                  .EndText();
            canvas.BeginText()
                  .SetFontAndSize(fontRegular, 7f)
                  .SetFillColor(new DeviceRgb(0.2f, 0.4f, 0.8f))
                  .MoveText(x + 45f, lineY)
                  .ShowText(signedDate)
                  .EndText();

            canvas.Release();

            // ─── Embed signature as PDF annotation / metadata ───
            // Store signature bytes in PDF custom metadata for audit
            var info = pdfDoc.GetDocumentInfo();
            info.SetMoreInfo("SignatureHex",  Convert.ToHexString(signatureBytes));
            info.SetMoreInfo("CertSerial",    certSerial);
            info.SetMoreInfo("CertSubject",   certSubject);
            info.SetMoreInfo("SignedBy",      ownerName);
            info.SetMoreInfo("SignedAt",      DateTime.UtcNow.ToString("O"));
        }

        await File.WriteAllBytesAsync(outputPath, dstMs.ToArray());
        _log.LogInformation("✅ Signed PDF written: {Path} ({Size} bytes)", outputPath, dstMs.Length);

        return outputPath;
    }

    private static string? ParseCN(string? dn)
    {
        if (string.IsNullOrEmpty(dn)) return null;
        var m = System.Text.RegularExpressions.Regex.Match(dn, @"CN=([^,]+)");
        return m.Success ? m.Groups[1].Value.Trim() : null;
    }

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s[..max] + "…";
    }
}
