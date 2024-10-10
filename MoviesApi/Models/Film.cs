namespace MoviesApi.Models
{
    public class Film
    {
        public int FilmID { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? DirectorID { get; set; }
        public int? StudioID { get; set; }
        public string? Review { get; set; }
        public int? CountryID { get; set; }
        public int? LanguageID { get; set; }
        public int? GenreID { get; set; }
        public short? RunTimeMinutes { get; set; }
        public int? CertificateID { get; set; }
        public long? BudgetDollars { get; set; }
        public long? BoxOfficeDollars { get; set; }
        public byte? OscarNominations { get; set; }
        public byte? OscarWins { get; set; }
    }

}
