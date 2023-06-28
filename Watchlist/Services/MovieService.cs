using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Security.Policy;
using Watchlist.Contracts;
using Watchlist.Data;
using Watchlist.Data.Models;
using Watchlist.Models;

namespace Watchlist.Services
{
    public class MovieService : IMovieService
    {
        private readonly WatchlistDbContext context;

        public MovieService(WatchlistDbContext _context)
        {
            context = _context;
        }

        public async Task AddFromImdbAsync(AddFromImdbVewModel model)
        {
            string apiKey = "c8267266";
            string movieTitle = model.Title;
            string apiUrl = $"http://www.omdbapi.com/?apikey={apiKey}&t={movieTitle}";
            // Send HTTP request to OMDB API
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    var content = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response to retrieve movie data
                    var movieData = JsonConvert.DeserializeObject<AddFromImdbVewModel>(content);

                    var entity = new Movie()
                    {
                        Director = movieData.Director,
                        GanreName = movieData.Genre,
                        ImageUrl = movieData.Poster,
                        Rating = 0.0m,
                        Title = movieData.Title
                    };
                    if (movieData.Title == null)
                    {
                        throw new ArgumentNullException("Movie not found!");
                    }
                    if (movieData.imdbRating == "N/A")
                    {
                        entity.Rating = 0.0m;
                    }
                    else
                    {
                        entity.Rating = decimal.Parse(movieData.imdbRating);
                    }
                    // Add the movie to the database


                    await context.Movies.AddAsync(entity);
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Failed to fetch movie details from the API.");
                }
            }
        }
        public async Task AddMovieAsync(AddMovieViewModel model)
        {
            var entity = new Movie()
            {
                Director = model.Director,
                GenreId = model.GenreId,
                ImageUrl = model.ImageUrl,
                Rating = model.Rating,
                Title = model.Title
            };

            await context.Movies.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task AddMovieToCollectionAsync(int movieId, string userId)
        {
            var user = await context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.UsersMovies)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ArgumentException("Invalid user ID");
            }

            var movie = await context.Movies.FirstOrDefaultAsync(u => u.Id == movieId);

            if (movie == null)
            {
                throw new ArgumentException("Invalid Movie ID");
            }

            if (!user.UsersMovies.Any(m => m.MovieId == movieId))
            {
                user.UsersMovies.Add(new UserMovie()
                {
                    MovieId = movie.Id,
                    UserId = user.Id,
                    Movie = movie,
                    User = user
                });
                movie.IsAddedToMineCollection = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MovieViewModel>> GetAllAsync()
        {
            var entities = await context.Movies
                .Include(m => m.Genre)
                .ToListAsync();

            return entities
                .Select(m => new MovieViewModel()
                {
                    Id = m.Id,
                    Director = m.Director,
                    Genre = m?.Genre?.Name,
                    ImdbGanres = m?.GanreName,
                    ImageUrl = m.ImageUrl,
                    Rating = m.Rating,
                    Title = m.Title,
                    IsRemoved = m.IsRemoved,
                    IsAddedToMineCollection = m.IsAddedToMineCollection,
                });
        }

        public async Task<IEnumerable<Genre>> GetGenresAsync()
        {
            return await context.Genres.ToListAsync();
        }

        public async Task<IEnumerable<MovieViewModel>> GetWatchedAsync(string userId)
        {
            var user = await context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.UsersMovies)
                .ThenInclude(um => um.Movie)
                .ThenInclude(m => m.Genre)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ArgumentException("Invalid user ID");
            }

            return user.UsersMovies
                .Select(m => new MovieViewModel()
                {
                    Director = m.Movie.Director,
                    Genre = m.Movie.Genre?.Name,
                    ImdbGanres = m.Movie.GanreName,
                    Id = m.MovieId,
                    ImageUrl = m.Movie.ImageUrl,
                    Title = m.Movie.Title,
                    Rating = m.Movie.Rating,
                    IsRemoved = m.Movie.IsRemoved,
                    IsAddedToMineCollection = m.Movie.IsAddedToMineCollection,
                });
        }

        public async Task RemoveMovieFromCollectionAsync(int movieId, string userId)
        {
            var user = await context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.UsersMovies)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ArgumentException("Invalid user ID");
            }

            var movie = user.UsersMovies.FirstOrDefault(m => m.MovieId == movieId);
            var movieToReturnFromCollection = context.Movies.FirstOrDefault(x => x.Id == movieId);

            if (movie != null)
            {
                movieToReturnFromCollection.IsAddedToMineCollection = false;
                user.UsersMovies.Remove(movie);
                await context.SaveChangesAsync();
            }
        }
        public async Task RemoveMovieAsync(int movieId)
        {
            var movie = await context.Movies
                .Where(m => m.Id == movieId)
                .FirstOrDefaultAsync();

            if (movie == null)
            {
                throw new ArgumentException("Invalid moive ID");
            }
            if (movie != null)
            {
                movie.IsRemoved = true;
                await context.SaveChangesAsync();
            }
        }
        public async Task<Movie> GetMovieById(int movieId)
        {
            var movie = await context.Movies
                .Where(m => m.Id == movieId)
                .FirstOrDefaultAsync();

            return movie;
        }
        public async Task EditMovie(EditViewModel model)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (movie == null)
            {
                throw new ArgumentException("Invalid movie ID");
            }
            if (movie != null)
            {
                movie.Title = model.Title;
                movie.Director = model.Director;
                movie.Rating = model.Rating;
                movie.GanreName = model.GanreName;
                await context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Invalid movie ID");
            }
        }
    }
}
