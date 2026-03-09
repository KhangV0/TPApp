using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Kernel.Pdf;
using iText.Layout.Font;

namespace TPApp.Services;

/// <summary>
/// Converts HTML contract content to a PDF using iText7 pdfHTML.
/// Supports Vietnamese Unicode via Windows Fonts (Times New Roman / Arial).
/// </summary>
public class HtmlToPdfService
{
    private readonly ILogger<HtmlToPdfService> _log;
    private readonly IWebHostEnvironment _env;

    public HtmlToPdfService(ILogger<HtmlToPdfService> log, IWebHostEnvironment env)
    {
        _log = log;
        _env = env;
    }

    /// <summary>Converts HTML string to a styled A4 PDF byte array.</summary>
    public byte[] Convert(string htmlContent, string? title = null)
    {
        _log.LogInformation("HtmlToPdf: converting HTML → PDF ({Len} chars)", htmlContent.Length);

        // Ensure full HTML document structure
        if (!htmlContent.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) &&
            !htmlContent.TrimStart().StartsWith("<html",     StringComparison.OrdinalIgnoreCase))
        {
            htmlContent = WrapInDocument(htmlContent, title);
        }

        var ms = new MemoryStream();

        {
            using var writer = new PdfWriter(ms, new WriterProperties());
            writer.SetCloseStream(false);
            using var pdfDoc = new PdfDocument(writer);

            pdfDoc.GetDocumentInfo().SetTitle(title ?? "Hợp đồng");
            pdfDoc.GetDocumentInfo().SetCreator("TechMart Platform");

            var props = new ConverterProperties();
            props.SetBaseUri("file:///" + _env.WebRootPath.Replace('\\', '/') + "/");

            // ── Font provider hỗ trợ Unicode / tiếng Việt ──────────────────
            // Tắt built-in fonts (thường chỉ support Latin), load thủ công
            var fontProv = new DefaultFontProvider(false, false, false);

            // 1. Font từ wwwroot/fonts/ nếu admin đặt font tùy chỉnh
            var customFontDir = Path.Combine(_env.WebRootPath, "fonts");
            if (Directory.Exists(customFontDir))
                fontProv.AddDirectory(customFontDir);

            // 2. Times New Roman + Arial từ Windows — hỗ trợ đầy đủ tiếng Việt
            var winFonts = new[]
            {
                @"C:\Windows\Fonts\times.ttf",    // Times New Roman Regular
                @"C:\Windows\Fonts\timesbd.ttf",  // Times New Roman Bold
                @"C:\Windows\Fonts\timesi.ttf",   // Times New Roman Italic
                @"C:\Windows\Fonts\timesbi.ttf",  // Times New Roman Bold Italic
                @"C:\Windows\Fonts\arial.ttf",    // Arial Regular
                @"C:\Windows\Fonts\arialbd.ttf",  // Arial Bold
                @"C:\Windows\Fonts\ariali.ttf",   // Arial Italic
                @"C:\Windows\Fonts\arialuni.ttf", // Arial Unicode MS (fallback tốt nhất)
            };
            foreach (var f in winFonts)
            {
                if (File.Exists(f))
                    fontProv.AddFont(f);
            }

            // 3. Standard PDF fonts (Latin fallback)
            fontProv.AddStandardPdfFonts();

            props.SetFontProvider(fontProv);

            HtmlConverter.ConvertToPdf(htmlContent, pdfDoc, props);
        }

        var result = ms.ToArray();
        ms.Dispose();

        _log.LogInformation("HtmlToPdf: generated {Size} bytes", result.Length);
        return result;
    }

    private static string WrapInDocument(string body, string? title)
    {
        var safeTitle = System.Net.WebUtility.HtmlEncode(title ?? "Hợp đồng");
        return
            "<!DOCTYPE html>\n<html lang=\"vi\">\n<head>\n" +
            "  <meta charset=\"UTF-8\"/>\n" +
            "  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>\n" +
            $"  <title>{safeTitle}</title>\n" +
            "  <style>\n" +
            "    body { font-family: 'Times New Roman', Arial, sans-serif; font-size: 13pt;" +
            "           line-height: 1.65; margin: 2cm; color: #111; }\n" +
            "    h1, h2, h3 { font-weight: bold; text-align: center; }\n" +
            "    h1 { font-size: 16pt; margin-bottom: 6pt; }\n" +
            "    h2 { font-size: 14pt; margin: 10pt 0 4pt; }\n" +
            "    h3 { font-size: 13pt; }\n" +
            "    p  { margin: 5pt 0; text-align: justify; }\n" +
            "    table { width: 100%; border-collapse: collapse; margin: 10pt 0; }\n" +
            "    th, td { border: 1px solid #888; padding: 5pt 8pt; font-size: 11pt; }\n" +
            "    th { background: #f0f0f0; font-weight: bold; }\n" +
            "    ul, ol { margin: 4pt 0 4pt 24pt; }\n" +
            "    li  { margin-bottom: 3pt; }\n" +
            "    .text-center { text-align: center; }\n" +
            "    .text-right  { text-align: right;  }\n" +
            "  </style>\n</head>\n<body>\n" +
            body +
            "\n</body>\n</html>";
    }
}
