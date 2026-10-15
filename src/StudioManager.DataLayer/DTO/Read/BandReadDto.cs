namespace StudioManager.DataLayer.DTO.Read
{
    public class BandReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string? Members { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Notes { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
