using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The lyrics of a song - saved on their own from the song page.
    /// </summary>
    public class SongLyricsDto
    {
        [MaxLength(20000)]
        public string? Lyrics { get; set; }
    }
}
