using System.Text;
using System.Text.RegularExpressions;

namespace TPApp.Helpers
{
    /// <summary>
    /// Helper class for Vietnamese text normalization and processing
    /// </summary>
    public static class VietnameseTextHelper
    {
        /// <summary>
        /// Common Vietnamese word mappings (non-accented → accented)
        /// Used for accent-insensitive search
        /// </summary>
        private static readonly Dictionary<string, string> WordMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            // Common words - Technology & Equipment
            { "may", "máy" },
            { "cong", "công" },
            { "nghe", "nghệ" },
            { "thiet", "thiết" },
            { "bi", "bị" },
            { "nuoc", "nước" },
            { "dien", "điện" },
            { "tu", "tủ" },
            { "lanh", "lạnh" },
            { "nong", "nóng" },
            
            // Common words - General
            { "viet", "việt" },
            { "nam", "nam" }, // already correct
            { "ha", "hà" },
            { "noi", "nội" },
            { "sai", "sài" },
            { "gon", "gòn" },
            { "da", "đà" },
            { "nang", "nẵng" },
            { "hue", "huế" },
            { "can", "cần" },
            { "tho", "thơ" },
            
            // Common words - Actions
            { "do", "đo" },
            { "kiem", "kiểm" },
            { "tra", "tra" }, // context-dependent
            { "xuat", "xuất" },
            { "nhap", "nhập" },
            { "san", "sản" },
            { "pham", "phẩm" },
            { "chat", "chất" },
            { "luong", "lượng" },
            
            // Common words - Descriptions
            { "cao", "cao" },
            { "thap", "thấp" },
            { "lon", "lớn" },
            { "nho", "nhỏ" },
            { "moi", "mới" },
            { "cu", "cũ" },
            { "tot", "tốt" },
            { "xau", "xấu" },
            
            // Add more mappings as needed based on search analytics
        };

        /// <summary>
        /// Normalize a search keyword by converting non-accented Vietnamese words to accented equivalents
        /// </summary>
        /// <param name="keyword">Original search keyword</param>
        /// <returns>Normalized keyword with Vietnamese accents</returns>
        public static string NormalizeKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return keyword;

            // Split keyword into words
            var words = keyword.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Normalize each word
            var normalizedWords = new List<string>();
            foreach (var word in words)
            {
                var normalizedWord = NormalizeWord(word);
                normalizedWords.Add(normalizedWord);
            }

            // Join back together
            return string.Join(" ", normalizedWords);
        }

        /// <summary>
        /// Normalize a single word
        /// </summary>
        private static string NormalizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return word;

            // Try to find exact match in dictionary (case-insensitive)
            if (WordMappings.TryGetValue(word, out var mappedWord))
            {
                return mappedWord;
            }

            // If no mapping found, return lowercase version
            // (FullText search is case-insensitive, so this helps with consistency)
            return word.ToLowerInvariant();
        }

        /// <summary>
        /// Add or update a word mapping
        /// Useful for runtime updates based on search analytics
        /// </summary>
        public static void AddMapping(string nonAccented, string accented)
        {
            WordMappings[nonAccented] = accented;
        }

        /// <summary>
        /// Get all current word mappings (for debugging/admin purposes)
        /// </summary>
        public static IReadOnlyDictionary<string, string> GetMappings()
        {
            return WordMappings;
        }
    }
}
