using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.ClientModels
{
    /// <summary>
    /// The customer of the studio, e.g. a label, a band itself or a person. A client has one or more bands
    /// and owns the projects.
    /// </summary>
    public class Client : BaseEntity, IHasPhoto
    {
        /// <summary>Picture of the client (logo or photo), path in the file storage.</summary>
        public string? PhotoPath { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }

        public ICollection<Band> Bands { get; set; } = new List<Band>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();

        /// <summary>
        /// The assistant who created the client - sees it even before it has a project. Null = created by an owner.
        /// Otherwise assistants see a client only through its projects.
        /// </summary>
        public int? CreatedByUserId { get; set; }
    }
}
