public class Artist
{
    public required string Name { get; set; }
}

public class Song
{
    public required string Name { get; set; }
    public required List<Artist> Artists { get; set; }
}
