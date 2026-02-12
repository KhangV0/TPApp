using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPApp.Entities
{
    [Table("NDAAgreements")]
    public class NDAAgreement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập bên A")]
        [StringLength(200)]
        public string BenA { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập bên B")]
        [StringLength(200)]
        public string BenB { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại NDA")]
        [StringLength(100)]
        public string LoaiNDA { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập thời hạn bảo mật")]
        [StringLength(100)]
        public string ThoiHanBaoMat { get; set; } = null!;

        [StringLength(100)]
        public string? XacNhanKySo { get; set; }

        public int? DuAnId { get; set; }

        [Required(ErrorMessage = "Bạn phải đồng ý điều khoản trước khi tiếp tục.")]
        public bool DaDongY { get; set; }

        public int StatusId { get; set; } = 1;

        [StringLength(450)]
        public string? NguoiTao { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? NguoiSua { get; set; }

        public DateTime? NgaySua { get; set; }
    }
}
