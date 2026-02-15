using System.ComponentModel.DataAnnotations;

namespace TPApp.ViewModel
{
    /// <summary>
    /// Request model for AI supplier suggestion endpoint.
    /// </summary>
    public class SuggestRequest
    {
        /// <summary>
        /// The buyer's technology request query
        /// </summary>
        [Required(ErrorMessage = "Input is required")]
        [StringLength(2000, ErrorMessage = "Input cannot exceed 2000 characters")]
        public string Input { get; set; } = string.Empty;
    }
}
