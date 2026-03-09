using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Kernel.Pdf;
using iText.Layout.Font;

namespace TPApp.Services;

/// <summary>
/// Converts HTML contract content to a PDF using iText7 pdfHTML.
/// Used when no uploaded file exists — generates a PDF from HtmlContent on the fly.
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

        // Use block form so we can call ms.ToArray() AFTER pdfDoc closes but BEFORE ms disposes
        {
            using var writer = new PdfWriter(ms, new WriterProperties());
            writer.SetCloseStream(false);
            using var pdfDoc = new PdfDocument(writer);

            pdfDoc.GetDocumentInfo().SetTitle(title ?? "Hợp đồng");
            pdfDoc.GetDocumentInfo().SetCreator("TechMart Platform");

            var props = new ConverterProperties();
            props.SetBaseUri("file:///" + _env.WebRootPath.Replace('\\', '/') + "/");
            var fontProv = new DefaultFontProvider(true, true, false);
            props.SetFontProvider(fontProv);

            HtmlConverter.ConvertToPdf(htmlContent, pdfDoc, props);
        } // pdfDoc & writer flushed + disposed here; ms still open

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
            $"  <title>{safeTitle}</title>\n" +
            "  <style>\n" +
            "    body { font-family: 'Times New Roman', Times, serif; font-size: 13pt;" +
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
