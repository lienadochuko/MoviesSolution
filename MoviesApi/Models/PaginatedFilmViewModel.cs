namespace MoviesApi.Models
{
    public class PaginatedFilmViewModel
    {
        public IEnumerable<Film2> Films { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        public int TotalPages { get; set;}
    }
}
