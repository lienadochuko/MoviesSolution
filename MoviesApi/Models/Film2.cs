using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MoviesApi.Models
{
	public class Film2
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int FilmID { get; set; }
		public string Title { get; set; }
		public string? Poster { get; set; }
		public string? Rating { get; set; }
		public int LikeCount { get; set; }
	}

}
