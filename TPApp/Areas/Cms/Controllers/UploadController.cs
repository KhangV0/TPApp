using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;

namespace TPApp.Areas.Cms.Controllers
{
    [Area("Cms")]
    [Authorize(Policy = "CmsAccess")]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _imageDomain;
        private readonly string[] _allowedExtensions;
        private readonly string[] _allowedNonImageExtensions;
        private readonly long _maxFileSize;
        private readonly int _maxImageWidth;
        private readonly List<(int W, int H)> _imageSizes;

        public UploadController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _imageDomain = config["AppSettings:ImageDomain"]?.TrimEnd('/') ?? "";
            _allowedExtensions = config.GetSection("AppSettings:AllowedExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
            _allowedNonImageExtensions = config.GetSection("AppSettings:AllowedNonImageExtensions").Get<string[]>()
                ?? new[] { ".pdf" };
            _maxFileSize = config.GetValue<long>("AppSettings:MaxFileSize", 5 * 1024 * 1024);
            _maxImageWidth = config.GetValue<int>("AppSettings:MaxImageWidth", 1200);

            // Read sizes from config: ["254-170", "108-84", ...]
            _imageSizes = new List<(int, int)>();
            var sizes = config.GetSection("AppSettings:ImageSizes").Get<string[]>() ?? Array.Empty<string>();
            foreach (var s in sizes)
            {
                var parts = s.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var w) && int.TryParse(parts[1], out var h))
                    _imageSizes.Add((w, h));
            }
        }

        /// <summary>
        /// General upload endpoint (CKEditor, media, etc.)
        /// POST /cms/Upload/Upload
        /// Images → resized to max 1200px width, keep original filename.
        /// PDF   → saved as-is.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile upload)
        {
            try
            {
                // ── Validate presence ──
                if (upload == null || upload.Length == 0)
                    return Json(new { error = new { message = "Không có file nào được chọn." } });

                // ── Validate size ──
                if (upload.Length > _maxFileSize)
                    return Json(new { error = new { message = $"File vượt quá {_maxFileSize / 1024 / 1024} MB." } });

                // ── Validate extension (whitelist only) ──
                var extension = Path.GetExtension(upload.FileName).ToLowerInvariant();
                var isImage = _allowedExtensions.Contains(extension);
                var isPdf = _allowedNonImageExtensions.Contains(extension);

                if (!isImage && !isPdf)
                    return Json(new { error = new { message = "Định dạng file không hợp lệ. Chỉ cho phép: jpg, png, webp, pdf." } });

                // ── Build date-organized path: uploads/yyyy/MM/dd ──
                var today = DateTime.UtcNow;
                var relativePath = Path.Combine("uploads", today.Year.ToString(),
                    today.Month.ToString("D2"), today.Day.ToString("D2"));
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);
                Directory.CreateDirectory(fullPath);

                // ── Keep original filename, sanitize ──
                var safeName = SanitizeFileName(Path.GetFileNameWithoutExtension(upload.FileName));
                var fileName = $"{safeName}{extension}";

                // If file exists, prepend short unique code
                var filePath = Path.Combine(fullPath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    var code = Guid.NewGuid().ToString("N")[..6];
                    fileName = $"{code}-{fileName}";
                    filePath = Path.Combine(fullPath, fileName);
                }

                if (isImage)
                {
                    // Save original file as-is (no resize, no format conversion)
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await upload.CopyToAsync(stream);

                    // Generate thumbnail variants (preserve aspect ratio)
                    stream.Position = 0;
                    using var image = await Image.LoadAsync(stream);
                    foreach (var (w, h) in _imageSizes)
                    {
                        using var clone = image.Clone(ctx =>
                            ctx.Resize(new ResizeOptions
                            {
                                Size = new Size(w, h),
                                Mode = ResizeMode.Max,
                                Sampler = KnownResamplers.Lanczos3
                            }));
                        var sizedPath = Path.Combine(fullPath, $"{w}-{h}-{fileName}");
                        await clone.SaveAsync(sizedPath, GetEncoderForExtension(extension));
                    }
                }
                else
                {
                    // PDF: save as-is
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await upload.CopyToAsync(stream);
                }

                // ── Return full URL with ImageDomain ──
                var datePart = $"{today.Year}/{today.Month:D2}/{today.Day:D2}";
                var fileUrl = $"{_imageDomain}/{datePart}/{fileName}";
                return Json(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = $"Upload lỗi: {ex.Message}" } });
            }
        }

        /// <summary>
        /// Upload image for QuyTrinhHinhAnh — resize to max 1200px width,
        /// keep original filename, prepend code if duplicate.
        /// POST /cms/Upload/UploadImage
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            try
            {
                if (upload == null || upload.Length == 0)
                    return Json(new { error = new { message = "Không có file nào được chọn." } });

                if (upload.Length > _maxFileSize)
                    return Json(new { error = new { message = $"File vượt quá {_maxFileSize / 1024 / 1024} MB." } });

                var extension = Path.GetExtension(upload.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                    return Json(new { error = new { message = "Chỉ cho phép: jpg, jpeg, png, webp." } });

                // Date-organized folder
                var today = DateTime.UtcNow;
                var relativePath = Path.Combine("uploads", today.Year.ToString(),
                    today.Month.ToString("D2"), today.Day.ToString("D2"));
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);
                Directory.CreateDirectory(fullPath);

                // Keep original filename, sanitize
                var safeName = SanitizeFileName(Path.GetFileNameWithoutExtension(upload.FileName));
                var fileName = $"{safeName}{extension}";

                // If file exists, prepend short unique code
                var filePath = Path.Combine(fullPath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    var code = Guid.NewGuid().ToString("N")[..6];
                    fileName = $"{code}-{fileName}";
                    filePath = Path.Combine(fullPath, fileName);
                }

                // Save original file as-is (no resize, no format conversion)
                using var stream = new FileStream(filePath, FileMode.Create);
                await upload.CopyToAsync(stream);

                // Generate thumbnail variants (preserve aspect ratio)
                stream.Position = 0;
                using var image = await Image.LoadAsync(stream);
                foreach (var (w, h) in _imageSizes)
                {
                    using var clone = image.Clone(ctx =>
                        ctx.Resize(new ResizeOptions
                        {
                            Size = new Size(w, h),
                            Mode = ResizeMode.Max,
                            Sampler = KnownResamplers.Lanczos3
                        }));
                    var sizedPath = Path.Combine(fullPath, $"{w}-{h}-{fileName}");
                    await clone.SaveAsync(sizedPath, GetEncoderForExtension(extension));
                }

                var datePart = $"{today.Year}/{today.Month:D2}/{today.Day:D2}";
                var fileUrl = $"{_imageDomain}/{datePart}/{fileName}";
                return Json(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = $"Upload lỗi: {ex.Message}" } });
            }
        }

        /// <summary>
        /// Returns the appropriate image encoder based on file extension.
        /// PNG → PngEncoder (preserves transparency), others → JpegEncoder Q100.
        /// </summary>
        private static IImageEncoder GetEncoderForExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".png" => new PngEncoder(),
                ".webp" => new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 100 },
                _ => new JpegEncoder { Quality = 100 }
            };
        }

        /// <summary>
        /// Remove spaces, special chars, keep only alphanumeric, dash, underscore.
        /// </summary>
        private static string SanitizeFileName(string name)
        {
            name = name.Trim();

            // Convert Vietnamese diacritics to ASCII
            name = RemoveVietnameseDiacritics(name);

            // Replace spaces and dots with dashes
            name = name.Replace(" ", "-").Replace(".", "-");

            // Keep only alphanumeric, dash, underscore
            name = Regex.Replace(name, @"[^a-zA-Z0-9\-_]", "");

            // Collapse multiple dashes
            name = Regex.Replace(name, @"-+", "-").Trim('-');

            return string.IsNullOrEmpty(name) ? "file" : name;
        }

        private static string RemoveVietnameseDiacritics(string text)
        {
            var map = new Dictionary<char, char>
            {
                // a
                {'à','a'},{'á','a'},{'ạ','a'},{'ả','a'},{'ã','a'},
                {'â','a'},{'ầ','a'},{'ấ','a'},{'ậ','a'},{'ẩ','a'},{'ẫ','a'},
                {'ă','a'},{'ằ','a'},{'ắ','a'},{'ặ','a'},{'ẳ','a'},{'ẵ','a'},
                // e
                {'è','e'},{'é','e'},{'ẹ','e'},{'ẻ','e'},{'ẽ','e'},
                {'ê','e'},{'ề','e'},{'ế','e'},{'ệ','e'},{'ể','e'},{'ễ','e'},
                // i
                {'ì','i'},{'í','i'},{'ị','i'},{'ỉ','i'},{'ĩ','i'},
                // o
                {'ò','o'},{'ó','o'},{'ọ','o'},{'ỏ','o'},{'õ','o'},
                {'ô','o'},{'ồ','o'},{'ố','o'},{'ộ','o'},{'ổ','o'},{'ỗ','o'},
                {'ơ','o'},{'ờ','o'},{'ớ','o'},{'ợ','o'},{'ở','o'},{'ỡ','o'},
                // u
                {'ù','u'},{'ú','u'},{'ụ','u'},{'ủ','u'},{'ũ','u'},
                {'ư','u'},{'ừ','u'},{'ứ','u'},{'ự','u'},{'ử','u'},{'ữ','u'},
                // y
                {'ỳ','y'},{'ý','y'},{'ỵ','y'},{'ỷ','y'},{'ỹ','y'},
                // d
                {'đ','d'},
                // UPPERCASE
                {'À','A'},{'Á','A'},{'Ạ','A'},{'Ả','A'},{'Ã','A'},
                {'Â','A'},{'Ầ','A'},{'Ấ','A'},{'Ậ','A'},{'Ẩ','A'},{'Ẫ','A'},
                {'Ă','A'},{'Ằ','A'},{'Ắ','A'},{'Ặ','A'},{'Ẳ','A'},{'Ẵ','A'},
                {'È','E'},{'É','E'},{'Ẹ','E'},{'Ẻ','E'},{'Ẽ','E'},
                {'Ê','E'},{'Ề','E'},{'Ế','E'},{'Ệ','E'},{'Ể','E'},{'Ễ','E'},
                {'Ì','I'},{'Í','I'},{'Ị','I'},{'Ỉ','I'},{'Ĩ','I'},
                {'Ò','O'},{'Ó','O'},{'Ọ','O'},{'Ỏ','O'},{'Õ','O'},
                {'Ô','O'},{'Ồ','O'},{'Ố','O'},{'Ộ','O'},{'Ổ','O'},{'Ỗ','O'},
                {'Ơ','O'},{'Ờ','O'},{'Ớ','O'},{'Ợ','O'},{'Ở','O'},{'Ỡ','O'},
                {'Ù','U'},{'Ú','U'},{'Ụ','U'},{'Ủ','U'},{'Ũ','U'},
                {'Ư','U'},{'Ừ','U'},{'Ứ','U'},{'Ự','U'},{'Ử','U'},{'Ữ','U'},
                {'Ỳ','Y'},{'Ý','Y'},{'Ỵ','Y'},{'Ỷ','Y'},{'Ỹ','Y'},
                {'Đ','D'}
            };

            var sb = new System.Text.StringBuilder(text.Length);
            foreach (var c in text)
                sb.Append(map.TryGetValue(c, out var r) ? r : c);
            return sb.ToString();
        }
    }
}
