using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.ClientModels
{
    public class Band : BaseEntity, IHasPhoto
    {
        /// <summary>Picture of the band (band photo or logo), path in the file storage.</summary>
        public string? PhotoPath { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Genre { get; set; }

        /// <summary>
        /// Free text, e.g. "Max - vocals, Lisa - drums". Can become its own table later if needed.
        /// </summary>
        public string? Members { get; set; }

        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Notes { get; set; }

        public int ClientId { get; set; }
        public Client? Client { get; set; }

        /// <summary>
        /// Projects the band takes part in (many-to-many, e.g. a split EP has several bands).
        /// </summary>
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
