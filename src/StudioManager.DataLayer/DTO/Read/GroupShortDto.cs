namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>A group with its name only, e.g. the groups of a project.</summary>
    public class GroupShortDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
