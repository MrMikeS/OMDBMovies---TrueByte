using Microsoft.AspNetCore.Mvc;
using OMDBMovies.Models;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace OMDBMovies.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public HomeController(IHttpClientFactory httpClientFactory, string apiKey)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = apiKey;
        }

        public IActionResult Index(List<OMDbMovie> movies)
        {
            return View(movies);
        }

       
        public async Task<IActionResult> SearchMovies(string searchInput)
        {
            string apiUrl = $"http://www.omdbapi.com/?apikey={_apiKey}&";
            if (string.IsNullOrWhiteSpace(searchInput))
            {
                // Handle when the searchInput is null or empty
                ModelState.AddModelError("searchInput", "Search input is required.");
                return View("Index");
            }

            // Split the search input into individual criteria using the comma as delimiter
            string[] searchCriteria = searchInput.Split(',');

            // Trim whitespace from each search criteria
            for (int i = 0; i < searchCriteria.Length; i++)
            {
                searchCriteria[i] = searchCriteria[i].Trim();
            }

            //urls
            string url = apiUrl;
            if (searchCriteria.Length >= 1 && !string.IsNullOrWhiteSpace(searchCriteria[0]))
            {
                url += $"s={searchCriteria[0]}&"; 
            }
            if (searchCriteria.Length >= 2 && !string.IsNullOrWhiteSpace(searchCriteria[1]))
            {
                url += $"y={searchCriteria[1]}&";
            }
            if (searchCriteria.Length >= 3 && !string.IsNullOrWhiteSpace(searchCriteria[2]))
            {
                url += $"type={searchCriteria[2]}&";
            }
          
            // Send API request
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                
            
                return View("Error");
            }

            // Read the API response
            var responseContent = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON response to a list of movies
            List<OMDbMovie> movies = new List<OMDbMovie>();

            try
            {
                var searchResult = JsonConvert.DeserializeObject<MovieSearchResult>(responseContent);
                if (searchResult.Search != null)
                {
                    foreach (var movie in searchResult.Search)
                    {
                        // Fetch the full details for each movie
                        var movieResponse = await httpClient.GetAsync($"{apiUrl}i={movie.imdbID}&plot=full");
                        if (movieResponse.IsSuccessStatusCode)
                        {
                            var movieContent = await movieResponse.Content.ReadAsStringAsync();
                            var fullMovie = JsonConvert.DeserializeObject<OMDbMovie>(movieContent);
                            movies.Add(fullMovie);
                        }
                    }
                }
            }
            catch (JsonSerializationException)
            {
                
                return RedirectToAction("Index");
            }

            return View("Index", movies);
        }
    }

    public class MovieSearchResult
    {
     public List<OMDbMovie>? Search { get; set; }
         
    }

    public class OMDbMovie
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public string Director { get; set; }
        public string Plot { get; set; }
        public string Poster { get; set; }
        public string imdbRating { get; set; }
        public string imdbID { get; set; }
        public string Type { get; set; }
        
    }
 
    public class MovieResponse
    {
        public OMDbMovie Movie { get; set; }
    }

}