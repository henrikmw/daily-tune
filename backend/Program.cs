using backend.Services; // Import the namespace where SpotifyService is located

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Log to console

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register SpotifyService for dependency injection
builder.Services.AddSingleton<SpotifyService>();

// Add HttpClient for SpotifyService
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add an endpoint for getting a song by its Spotify ID with logging
app.MapGet("/api/songs/{songId}", async (string songId, SpotifyService spotifyService, ILogger<Program> logger) =>
{
    var song = await spotifyService.GetSongByIdAsync(songId);

    // Log the song details
    if (song.Name != null)
    {
        logger.LogInformation("Retrieved song: {@Song}", song.Name);
    }
    else
    {
        logger.LogWarning("Song with ID {SongId} not found.", songId);
    }

    if (song?.Name == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(song);
})
.WithName("GetSongById")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
