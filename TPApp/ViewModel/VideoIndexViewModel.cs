using TPApp.Entities;

namespace TPApp.ViewModel
{
    public class VideoIndexViewModel
    {
        public Content? Highlight { get; set; }

        public List<Content> Videos { get; set; } = new();

        public int TotalRecords { get; set; }
        public int PageSize { get; set; } = 6;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public List<int> Pages { get; set; } = new();
    }
}
