namespace StudioManager.DataLayer.DTO.Read
{
    public class InputGroupReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "gray";
        public int SortOrder { get; set; }

        /// <summary>Number of studio inputs in this group.</summary>
        public int InputCount { get; set; }
    }
}
