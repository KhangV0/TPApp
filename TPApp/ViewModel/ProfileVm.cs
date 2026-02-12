using System.ComponentModel.DataAnnotations;

namespace TPApp.ViewModel
{
    public class ProfileVm
    {
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime? Created { get; set; }
    }
}
