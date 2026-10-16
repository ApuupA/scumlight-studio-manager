namespace StudioManager.DataLayer.DTO.Read
{
    public class ProjectInstrumentReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsChain { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
        public int ProjectId { get; set; }
        public int? BandId { get; set; }
        public string? BandName { get; set; }

        /// <summary>The setup, in chain / list order (filled by ProjectInstrumentService).</summary>
        public List<InstrumentSetupItemReadDto> SetupItems { get; set; } = new();

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
