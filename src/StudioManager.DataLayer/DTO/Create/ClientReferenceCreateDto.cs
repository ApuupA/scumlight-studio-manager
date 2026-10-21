using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// A reference track the client adds on the client page, e.g. "Mastodon - Blood and Thunder" with a Spotify link.
    /// </summary>
    public class ClientReferenceCreateDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, Url, MaxLength(2000)]
        public string Url { get; set; } = string.Empty;

        /// <summary>What to listen for, e.g. "the snare sound".</summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>Optional: the song of the project it is a reference for.</summary>
        public int? SongId { get; set; }

        [Required, MaxLength(100)]
        public string AuthorName { get; set; } = string.Empty;
    }
}
