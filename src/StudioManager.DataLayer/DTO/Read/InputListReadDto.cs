namespace StudioManager.DataLayer.DTO.Read
{
    public class InputListReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int SortOrder { get; set; }
        public int ProjectId { get; set; }

        /// <summary>
        /// One row per studio input (in input order) - filled ones with their values, the others empty.
        /// Built by InputListService, so frontend and print view get the complete table.
        /// </summary>
        public List<InputListRowReadDto> Rows { get; set; } = new();

        /// <summary>Number of inputs in use.</summary>
        public int UsedInputCount { get; set; }
    }
}
