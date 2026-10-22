using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// A group of the studio with its members (settings, only the owner).
    /// </summary>
    public class GroupCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>The logins in the group.</summary>
        public List<int> UserIds { get; set; } = new();
    }
}
