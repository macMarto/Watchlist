using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = Microsoft.Build.Framework.RequiredAttribute;

namespace Watchlist.Models
{

	public class AddFromImdbVewModel
	{
        [Required]
        public string Title { get; set; }
        public string? Genre { get; set; }
        public string? Director { get; set; }
        public string? Poster { get; set; }
        public string? imdbRating { get; set; }
    
    }
}
