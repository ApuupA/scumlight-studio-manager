namespace StudioManager.DataLayer.DTO.Read
{
    public class SetupTemplateItemReadDto
    {
        public int Id { get; set; }
        public int? GearItemId { get; set; }

        /// <summary>Gear name or the free text.</summary>
        public string DisplayName { get; set; } = string.Empty;

        public string? CustomName { get; set; }
        public string? UsedFor { get; set; }
        public int Quantity { get; set; }
        public bool NeedsInput { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }
}
