using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPApp.Enums;

namespace TPApp.Entities
{
    [Table("NegotiationForms")]
    public class NegotiationForm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? RFQId { get; set; }

        public int? ProjectId { get; set; }

        public int SellerId { get; set; } // Selected seller (no FK)

        public decimal? GiaChotCuoiCung { get; set; }

        // Nullable for auto-created draft records
        public string? DieuKhoanThanhToan { get; set; }

        public string? BienBanThuongLuongFile { get; set; }

        [StringLength(50)]
        public string? HinhThucKy { get; set; }

        public bool DaKySo { get; set; } = false;

        // Two-party signature flags
        public bool SellerSigned { get; set; } = false;
        public bool BuyerSigned  { get; set; } = false;

        // OTP fields
        [StringLength(10)]
        public string? SellerOtpCode    { get; set; }
        public DateTime? SellerOtpExpire { get; set; }

        [StringLength(10)]
        public string? BuyerOtpCode     { get; set; }
        public DateTime? BuyerOtpExpire  { get; set; }

        // Signature timestamps
        public DateTime? SellerSignedAt { get; set; }
        public DateTime? BuyerSignedAt  { get; set; }

        // StatusId: 1=Draft, 2=PriceAgreed, 3=WaitingSignature, 4=PartiallySigned, 5=Completed
        public int StatusId { get; set; } = (int)NegotiationStatus.Draft;

        public int? NguoiTao { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public int? NguoiSua { get; set; }
        public DateTime? NgaySua { get; set; }
    }
}
