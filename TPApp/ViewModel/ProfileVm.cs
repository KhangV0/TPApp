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

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime? LastLogin { get; set; }
        public DateTime? Created { get; set; }

        // Account type
        /// <summary>1 = Cá nhân, 2 = Doanh nghiệp</summary>
        public int AccountTypeId { get; set; } = 1;

        // Verification status  
        public bool PhoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        public int VerificationLevel { get; set; }

        // Uploaded documents
        public List<VerifyDocVm> Docs { get; set; } = new();
    }

    public class VerifyDocVm
    {
        public int Id { get; set; }
        /// <summary>1=CCCD_Front, 2=CCCD_Back, 3=BusinessLicense</summary>
        public int DocType { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public int ReviewStatus { get; set; }
    }
}
