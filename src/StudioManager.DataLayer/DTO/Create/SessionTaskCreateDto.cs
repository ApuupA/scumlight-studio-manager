using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class SessionTaskCreateDto
    {
        [Required, MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        public bool IsDone { get; set; }
        public int SortOrder { get; set; }

        [Range(1, int.MaxValue)]
        public int SessionId { get; set; }
    }
}
