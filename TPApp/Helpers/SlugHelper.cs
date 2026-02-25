using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TPApp.Helpers
{
    public static class SlugHelper
    {
        /// <summary>
        /// Convert Vietnamese text to URL-friendly slug.
        /// "Công nghệ mới 2024" → "cong-nghe-moi-2024"
        /// </summary>
        public static string Slugify(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            // Remove Vietnamese diacritics
            text = RemoveVietnameseDiacritics(text);

            // Normalize unicode
            text = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in text)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            text = sb.ToString().Normalize(NormalizationForm.FormC);

            // Lowercase
            text = text.ToLowerInvariant();

            // Replace spaces and non-alphanum with dash
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"[\s]+", "-");
            text = Regex.Replace(text, @"-{2,}", "-");
            text = text.Trim('-');

            return text;
        }

        private static string RemoveVietnameseDiacritics(string text)
        {
            // Vietnamese-specific character mapping
            var from = "àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ" +
                       "ÀÁẠẢÃÂẦẤẬẨẪĂẰẮẶẲẴÈÉẸẺẼÊỀẾỆỂỄÌÍỊỈĨÒÓỌỎÕÔỒỐỘỔỖƠỜỚỢỞỠÙÚỤỦŨƯỪỨỰỬỮỲÝỴỶỸĐ";
            var to   = "aaaaaaaaaaaaaaaaaeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyd" +
                       "AAAAAAAAAAAAAAAAAEEEEEEEEEEEIIIIIOOOOOOOOOOOOOOOOOUUUUUUUUUUUYYYYYD";

            for (int i = 0; i < from.Length; i++)
            {
                text = text.Replace(from[i], to[i]);
            }
            return text;
        }
    }
}
