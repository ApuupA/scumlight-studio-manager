using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.UserModels
{
    /// <summary>
    /// A team of the studio, e.g. "Sacroscum" - the assistants in the group see the projects of the group.
    /// A user can be in several groups, a project can belong to several groups. The owner sees every project anyway.
    /// </summary>
    public class Group : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>The members (many-to-many, join table "UserGroups").</summary>
        public ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>The projects of the group (many-to-many, join table "ProjectGroups").</summary>
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
