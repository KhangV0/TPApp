using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("Store")]
    public class Store
    {
        public int StoreId { get; set; }

        public string? Title { get; set; }
        public string? QueryString { get; set; }
        public string? Slogan { get; set; }
        public string? Description { get; set; }
        public string? ContactUs { get; set; }
        public string? URLWEB { get; set; }
        public string? Email { get; set; }
        public string? ImgLogo { get; set; }

        public int? TemplateId { get; set; }
        public int? StoreTypeId { get; set; }
        public int? UserId { get; set; }

        public float? Viewed { get; set; }
        public float? Mark { get; set; }
        public int? TotalVote { get; set; }

        public bool? IsHot { get; set; }
        public int? StatusId { get; set; }

        public DateTime? bEffectiveDate { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? eEffectiveDate { get; set; }

        public string? BigBanner { get; set; }
        public string? ImageSlider { get; set; }

        public string? YahooId { get; set; }
        public string? SkypeId { get; set; }

        public int? TinhThanhId { get; set; }
        public string? Code { get; set; }
        public string? DiaChi { get; set; }

        public string? HoTen { get; set; }
        public string? Phone { get; set; }
        public string? PhoneOther { get; set; }

        public bool? IsNewCar { get; set; }
        public bool? IsOldCar { get; set; }
        public bool? IsVip { get; set; }

        public string? HeaderImage { get; set; }
        public string? Map { get; set; }

        public DateTime? OrderDate { get; set; }
        public int? DailyOrder { get; set; }

        public string Domain { get; set; } = null!;

        public int? LanguageId { get; set; }
        public int? ParentId { get; set; }
        public int? SiteId { get; set; }
    }
}
