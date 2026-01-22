using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.IO;

namespace TPApp.Web.Helpers
{
    public static class ImageHtmlHelper
    {
        // ===============================
        // IMAGE URL (giữ logic WebForms)
        // ===============================
        public static string CookedImageURL(
    this IHtmlHelper html,
    string size,
    string? imageUrl)
        {
            var config = html.ViewContext.HttpContext
                .RequestServices
                .GetService(typeof(IConfiguration)) as IConfiguration;

            var mainDomain = config?["AppSettings:MainDomain"] ?? "";

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return $"{mainDomain.TrimEnd('/')}/images/{size}_noImage.jpg";
            }

            if (!imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                imageUrl = $"{mainDomain.TrimEnd('/')}/{imageUrl.TrimStart('/')}";
            }

            var fileName = Path.GetFileName(imageUrl);

            // ✅ size = org → trả về file gốc, không prefix
            if (string.Equals(size, "org", StringComparison.OrdinalIgnoreCase))
            {
                return imageUrl;
            }

            // Tránh double size
            if (fileName.StartsWith(size + "-", StringComparison.OrdinalIgnoreCase))
            {
                return imageUrl;
            }

            return imageUrl.Replace(fileName, $"{size}-{fileName}");
        }


        // ===============================
        // MAKE URL FRIENDLY (SEO)
        // ===============================
        public static string MakeURLFriendly(
            this IHtmlHelper html,
            string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var str = input.ToLower().Trim();
            var old = str;

            // Bảng chuyển dấu tiếng Việt (giữ đúng VB.NET)
            const string findText =
                "ä|à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ|" +
                "ç|" +
                "è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ|" +
                "ì|í|î|ị|ỉ|ĩ|" +
                "ö|ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ|" +
                "ü|ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ|" +
                "ỳ|ý|ỵ|ỷ|ỹ|" +
                "đ";

            const string replaceText =
                "a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|a|" +
                "c|" +
                "e|e|e|e|e|e|e|e|e|e|e|" +
                "i|i|i|i|i|i|" +
                "o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|o|" +
                "u|u|u|u|u|u|u|u|u|u|u|u|" +
                "y|y|y|y|y|" +
                "d";

            var findArr = findText.Split('|');
            var replaceArr = replaceText.Split('|');

            for (int i = 0; i < findArr.Length; i++)
            {
                str = str.Replace(findArr[i], replaceArr[i]);
            }

            // Thay ký tự đặc biệt bằng "-"
            str = Regex.Replace(str, @"[^a-z0-9]", "-");

            // Gom dấu "-"
            str = Regex.Replace(str, @"-+", "-").Trim('-');

            // Trường hợp tiếng Hán / quá ngắn (giữ logic cũ)
            if (str.Length < 3)
            {
                str = old
                    .Replace(" ", "-")
                    .Replace(".", "-")
                    .Replace("?", "-");
            }

            return str;
        }
    }
}
