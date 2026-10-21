using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The details of a voice memo. The memo itself is created by the upload (POST api/VoiceMemo/project/{projectId}/upload),
    /// this DTO changes title, notes, date and where it belongs (song and part).
    /// </summary>
    public class VoiceMemoCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public DateOnly? RecordedOn { get; set; }

        /// <summary>Null = not sorted yet (inbox).</summary>
        public int? SongId { get; set; }

        /// <summary>A part of the song's arrangement - only together with SongId.</summary>
        public int? SongSectionId { get; set; }
    }
}
