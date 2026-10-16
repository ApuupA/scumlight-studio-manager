namespace StudioManager.DataLayer.DTO.Read
{
    public class SetupTemplateReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsChain { get; set; }
        public string? Notes { get; set; }

        /// <summary>The entries, in chain / list order.</summary>
        public List<SetupTemplateItemReadDto> Items { get; set; } = new();
    }
}
