namespace TPApp.ViewModel
{
    /// <summary>
    /// DTO for proposal submission
    /// </summary>
    public class ProposalSubmissionDto
    {
        public string GiaiPhapKyThuat { get; set; } = string.Empty;
        public decimal? BaoGiaSoBo { get; set; }
        public string ThoiGianTrienKhai { get; set; } = string.Empty;
        public string HoSoNangLucDinhKem { get; set; } = string.Empty;
    }
}
