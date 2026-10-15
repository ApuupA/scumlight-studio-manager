namespace StudioManager.DataLayer.DTO.Read
{
    public class ExternalGearReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Owner { get; set; }
        public string? Notes { get; set; }
        public int ProjectId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
