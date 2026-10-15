using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class ExternalGearCreateDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        [MaxLength(200)]
        public string? Owner { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }
    }
}
