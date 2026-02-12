using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace TPApp.Helpers
{
    public static class ImageHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png" };
        private const long MaxFileSize = 2 * 1024 * 1024; // 2MB

        public static bool ValidateImage(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Vui lòng chọn file ảnh";
                return false;
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                errorMessage = "Kích thước file không được vượt quá 2MB";
                return false;
            }

            // Check extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = "Chỉ chấp nhận file ảnh định dạng JPG, JPEG, PNG";
                return false;
            }

            // Check MIME type
            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                errorMessage = "Loại file không hợp lệ";
                return false;
            }

            return true;
        }

        public static async Task<string> ResizeAndSaveImageAsync(IFormFile file, string savePath, int width, int height)
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());
            
            // Resize image
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Crop
            }));

            // Save as JPEG with quality 85
            await image.SaveAsJpegAsync(savePath, new JpegEncoder { Quality = 85 });

            return savePath;
        }

        public static void DeleteOldAvatar(string? avatarPath, IWebHostEnvironment environment)
        {
            if (string.IsNullOrEmpty(avatarPath))
                return;

            // Don't delete default avatar
            if (avatarPath.Contains("default-avatar"))
                return;

            var fullPath = Path.Combine(environment.WebRootPath, avatarPath.TrimStart('/'));
            
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    // Ignore deletion errors
                }
            }
        }
    }
}
