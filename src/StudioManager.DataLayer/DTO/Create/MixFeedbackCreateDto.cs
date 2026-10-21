using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// One point of feedback on a mix version, e.g. Timecode "1:23", Text "Vocals too quiet".
    /// </summary>
    public class MixFeedbackCreateDto
    {
        [Range(1, int.MaxValue)]
        public int MixVersionId { get; set; }

        [MaxLength(10)]
        public string? Timecode { get; set; }

        [Required, MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        public bool IsDone { get; set; }
    }
}
