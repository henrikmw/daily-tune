using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private readonly SpotifyService _spotifyService;
        private readonly ILogger<SongsController> _logger; // Logger

        public SongsController(SpotifyService spotifyService, ILogger<SongsController> logger)
        {
            _spotifyService = spotifyService;
            _logger = logger; // Initialize the logger
        }

        [HttpGet("{songId}")]
        public async Task<IActionResult> GetSong(string songId)
        {
            var song = await _spotifyService.GetSongByIdAsync(songId);
            
            _logger.LogInformation("Retrieved song: {@Song}", song);

            if (song.Name == null)
            {
                return NotFound();
            }
            
            return Ok(song);
        }
    }
}
