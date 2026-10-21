namespace StudioManager.DataLayer.DTO.Read
{
    public class ProjectDeliverableReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public string? Notes { get; set; }
        public int SortOrder { get; set; }
        public int ProjectId { get; set; }
    }
}
