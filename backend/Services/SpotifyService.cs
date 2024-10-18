using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace backend.Services
{
    public class SpotifyService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public SpotifyService(IConfiguration config, ILogger<SpotifyService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = new HttpClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var clientId = _config["Spotify:ClientId"];
            var clientSecret = _config["Spotify:ClientSecret"];
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("Spotify Client ID or Client Secret is not configured.");
            }

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<SpotifyTokenResponse>(responseBody);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Failed to retrieve access token from Spotify.");
                throw new InvalidOperationException("Could not retrieve Spotify access token.");
            }

            _logger.LogInformation("Retrieved token: {@Token}", tokenResponse.AccessToken);
            return tokenResponse.AccessToken;
        }

        public async Task<Song> GetSongByIdAsync(string songId)
        {
            var token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/tracks/{songId}");
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve song from Spotify. Status Code: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Spotify API request failed with status code {response.StatusCode}");
            }

            var song = JsonConvert.DeserializeObject<Song>(responseBody);

            if (song == null || string.IsNullOrEmpty(song.Name))
            {
                _logger.LogError("Song not found or invalid response from Spotify API.");
                throw new InvalidOperationException("Failed to retrieve song from Spotify.");
            }

            return song;
        }
    }

    public class SpotifyTokenResponse
    {
        [JsonProperty("access_token")]
        public required string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public required string TokenType { get; set; }
    }
}
