namespace TPApp.ViewModel
{
    // =====================================================================
    // NHA CUNG UNG — INDEX
    // =====================================================================
    public class NhaCungUngIndexVm
    {
        public string PageTitle { get; set; } = "Nhà cung ứng";

        // Filter
        public int CateId      { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize    { get; set; } = 16;
        public int TotalCount  { get; set; }
        public int TotalPage   => TotalCount == 0 ? 1
            : (int)Math.Ceiling((double)TotalCount / PageSize);

        // Visible page numbers
        public List<int> Pages { get; set; } = new();

        // Data
        public List<NhaCungUngItemVm> Items      { get; set; } = new();
        public List<NhaCungUngCateVm> Categories { get; set; } = new();
    }

    // =====================================================================
    // NHA CUNG UNG — ITEM (used in list + sidebar "khác")
    // =====================================================================
    public class NhaCungUngItemVm
    {
        public int    Id       { get; set; }
        public string FullName { get; set; } = "";
        public string Slug     { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string DiaChi   { get; set; } = "";
        public string Phone    { get; set; } = "";
        public string Email    { get; set; } = "";
        public string Website  { get; set; } = "";
        public int    Rating   { get; set; }

        // Derived URL
        public string DetailUrl => $"/nha-cung-ung/{Slug}-{Id}.html";
    }

    // =====================================================================
    // NHA CUNG UNG — CATEGORY (sidebar filter)
    // =====================================================================
    public class NhaCungUngCateVm
    {
        public int    Id    { get; set; }
        public string Title { get; set; } = "";
    }

    // =====================================================================
    // NHA CUNG UNG — DETAIL
    // =====================================================================
    public class NhaCungUngDetailVm
    {
        public int    Id       { get; set; }
        public string Slug     { get; set; } = "";
        public string FullName { get; set; } = "";
        public string ImageUrl { get; set; } = "";

        // Company info
        public string DiaChi         { get; set; } = "";
        public string Phone          { get; set; } = "";
        public string Fax            { get; set; } = "";
        public string Email          { get; set; } = "";
        public string Website        { get; set; } = "";
        public string NguoiDaiDien   { get; set; } = "";
        public string ChucVu         { get; set; } = "";
        public string ChucNang       { get; set; } = "";
        public string SanPham        { get; set; } = "";
        public string LinhVucText    { get; set; } = "";   // resolved from LinhVucId IDs
        public string DichVuText     { get; set; } = "";   // resolved from DichVu IDs

        // Stats
        public int Rating      { get; set; }
        public int LuotXem     { get; set; }
        public int LuotDanhGia { get; set; }

        // Products of this supplier
        public List<NhaCungUngProductVm> Products { get; set; } = new();

        // Sidebar
        public List<NhaCungUngItemVm> NhaCungUngKhac { get; set; } = new();
        public List<NhaCungUngCateVm> Categories     { get; set; } = new();
    }

    // =====================================================================
    // NHA CUNG UNG — PRODUCT ITEM (for detail page product grid)
    // =====================================================================
    public class NhaCungUngProductVm
    {
        public int    Id       { get; set; }
        public string Title    { get; set; } = "";
        public string Code     { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Url      { get; set; } = "";
        public string PriceText { get; set; } = "";
        public int    Rating   { get; set; }
        public int    ProductType { get; set; }
    }
}
