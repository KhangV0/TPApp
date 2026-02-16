using System.Text.RegularExpressions;

namespace TPApp.Application.Helpers
{
    /// <summary>
    /// Helper class for highlighting keywords and creating snippets in search results
    /// </summary>
    public static class SearchHighlightHelper
    {
        /// <summary>
        /// Highlight keywords in text with &lt;mark&gt; tags
        /// </summary>
        /// <param name="text">Text to highlight</param>
        /// <param name="keywords">Keywords to highlight (space-separated)</param>
        /// <returns>Text with highlighted keywords</returns>
        public static string HighlightKeywords(string text, string keywords)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keywords))
                return text;

            // Normalize keywords for Vietnamese accent-insensitive matching
            var normalizedKeywords = TPApp.Helpers.VietnameseTextHelper.NormalizeKeyword(keywords);
            var words = normalizedKeywords.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var highlighted = text;

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                    continue;

                // Match whole words only, case-insensitive
                var pattern = $@"\b({Regex.Escape(word)})\b";
                highlighted = Regex.Replace(
                    highlighted,
                    pattern,
                    "<mark>$1</mark>",
                    RegexOptions.IgnoreCase);
            }

            return highlighted;
        }

        /// <summary>
        /// Create a snippet around the first occurrence of keywords
        /// </summary>
        /// <param name="text">Full text</param>
        /// <param name="keywords">Keywords to find (space-separated)</param>
        /// <param name="maxLength">Maximum snippet length</param>
        /// <returns>Snippet with highlighted keywords</returns>
        public static string CreateSnippet(string text, string keywords, int maxLength = 200)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(keywords))
                return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;

            var words = keywords.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstWord = words.FirstOrDefault();

            if (firstWord == null)
                return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;

            // Find first occurrence of any keyword
            var index = text.IndexOf(firstWord, StringComparison.OrdinalIgnoreCase);

            if (index == -1)
            {
                // Keyword not found, return beginning of text
                return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
            }

            // Calculate snippet boundaries
            var start = Math.Max(0, index - maxLength / 2);
            var length = Math.Min(maxLength, text.Length - start);

            // Adjust start to word boundary
            if (start > 0)
            {
                var spaceIndex = text.IndexOf(' ', start);
                if (spaceIndex > 0 && spaceIndex < start + 20)
                    start = spaceIndex + 1;
            }

            var snippet = text.Substring(start, length);

            // Add ellipsis
            if (start > 0)
                snippet = "..." + snippet;

            if (start + length < text.Length)
                snippet += "...";

            // Highlight keywords in snippet
            return HighlightKeywords(snippet, keywords);
        }

        /// <summary>
        /// Remove HTML tags from text
        /// </summary>
        public static string StripHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return Regex.Replace(text, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Truncate text to specified length
        /// </summary>
        public static string Truncate(string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + suffix;
        }
    }
}
