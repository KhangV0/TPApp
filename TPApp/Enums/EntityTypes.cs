namespace TPApp.Enums
{
    /// <summary>
    /// Centralized entity type constants. No magic strings allowed.
    /// </summary>
    public static class EntityTypes
    {
        public const string SanPhamCNTB = "SanPhamCNTB";
        public const string NhaCungUng  = "NhaCungUng";
        public const string NhaTuVan    = "NhaTuVan";
        public const string Contents    = "Contents";

        /// <summary>Whitelist of valid entity types.</summary>
        public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
        {
            SanPhamCNTB, NhaCungUng, NhaTuVan, Contents
        };

        public static bool IsValid(string? type) => !string.IsNullOrEmpty(type) && All.Contains(type);
    }
}
