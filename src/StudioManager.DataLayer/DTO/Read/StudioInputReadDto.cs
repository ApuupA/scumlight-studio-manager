namespace StudioManager.DataLayer.DTO.Read
{
    public class StudioInputReadDto
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public string? SnakeChannel { get; set; }
        public string? Preamp { get; set; }
        public string? DawChannel { get; set; }
        public string? Notes { get; set; }
        public int? InputGroupId { get; set; }
        public string? InputGroupName { get; set; }
        public string? InputGroupColor { get; set; }
    }
}
