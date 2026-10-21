namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// Just id, track number and title of a song, used inside other DTOs (e.g. the songs of a mix version).
    /// </summary>
    public class SongShortDto
    {
        public int Id { get; set; }
        public int TrackNumber { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
