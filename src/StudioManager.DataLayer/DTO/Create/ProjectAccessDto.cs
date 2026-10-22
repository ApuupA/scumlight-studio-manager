namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Who sees the project (only the owner may change it): its groups, and "visible to everyone".
    /// </summary>
    public class ProjectAccessDto
    {
        public List<int> GroupIds { get; set; } = new();

        /// <summary>Every assistant sees the project, no matter in which groups.</summary>
        public bool IsVisibleToAll { get; set; }
    }
}
