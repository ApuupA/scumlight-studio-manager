using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class ClientCreateDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }
    }
}
