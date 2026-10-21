using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Feedback the client writes on the client page, e.g. song 3, "1:23", "Vocals too quiet", from "Simon".
    /// </summary>
    public class ClientFeedbackCreateDto
    {
        [Range(1, int.MaxValue)]
        public int MixVersionId { get; set; }

        /// <summary>The song it is about (must be in the version) - null for general feedback.</summary>
        public int? SongId { get; set; }

        [MaxLength(10)]
        public string? Timecode { get; set; }

        [Required, MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        /// <summary>The name the client enters (remembered in the browser).</summary>
        [Required, MaxLength(100)]
        public string AuthorName { get; set; } = string.Empty;
    }
}
