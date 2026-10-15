using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class BandCreateDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Genre { get; set; }

        [MaxLength(1000)]
        public string? Members { get; set; }

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Website { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int ClientId { get; set; }
    }
}
