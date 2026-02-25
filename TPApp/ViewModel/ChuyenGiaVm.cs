using TPApp.Entities;

namespace TPApp.ViewModel
{
    // =====================================================================
    // CHUYEN GIA — INDEX
    // =====================================================================
    public class ChuyenGiaIndexVm
    {
        public string PageTitle { get; set; } = "Chuyên gia tư vấn";

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
        public List<ChuyenGiaItemVm>   Items      { get; set; } = new();
        public List<ChuyenGiaCateVm>   Categories { get; set; } = new();
    }

    // =====================================================================
    // CHUYEN GIA — ITEM (used in list + sidebar "khác")
    // =====================================================================
    public class ChuyenGiaItemVm
    {
        public int    Id       { get; set; }
        public string FullName { get; set; } = "";
        public string Slug     { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string CoQuan   { get; set; } = "";
        public string ChucVu   { get; set; } = "";
        public string Phone    { get; set; } = "";
        public string Email    { get; set; } = "";
        public int    Rating   { get; set; }

        // Derived URL
        public string DetailUrl => $"/chuyen-gia/{Slug}-{Id}.html";
    }

    // =====================================================================
    // CHUYEN GIA — CATEGORY (sidebar filter)
    // =====================================================================
    public class ChuyenGiaCateVm
    {
        public int    Id    { get; set; }
        public string Title { get; set; } = "";
    }

    // =====================================================================
    // CHUYEN GIA — DETAIL
    // =====================================================================
    public class ChuyenGiaDetailVm
    {
        public int    Id       { get; set; }
        public string Slug     { get; set; } = "";
        public string FullName { get; set; } = "";
        public string ImageUrl { get; set; } = "";

        // Profile info
        public string DateOfBirth        { get; set; } = "";
        public string DiaChi             { get; set; } = "";
        public string Phone              { get; set; } = "";
        public string Email              { get; set; } = "";
        public string HocHam             { get; set; } = "";
        public string CoQuan             { get; set; } = "";
        public string ChucVu             { get; set; } = "";
        public string DichVuText         { get; set; } = "";   // resolved from DichVu IDs
        public string LinhVucText        { get; set; } = "";   // resolved from LinhVucId IDs
        public string KetQuaNghienCuu    { get; set; } = "";

        // Stats
        public int Rating      { get; set; }
        public int LuotXem     { get; set; }
        public int LuotDanhGia { get; set; }

        // Tags
        public List<string> TuKhoa { get; set; } = new();

        // Sidebar
        public List<ChuyenGiaItemVm> NhaTuVanKhac { get; set; } = new();
        public List<ChuyenGiaCateVm> Categories   { get; set; } = new();
    }
}
