namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// Result of "fill from setups": the updated list and how many microphones / DIs were placed.
    /// </summary>
    public class InputListFillResultDto
    {
        public InputListReadDto InputList { get; set; } = new();

        /// <summary>Rows added to free inputs.</summary>
        public int AddedCount { get; set; }

        /// <summary>Microphones / DIs that did not fit because all inputs are in use.</summary>
        public int NotPlacedCount { get; set; }
    }
}
