namespace StudioManager.DataLayer.Models.SettingsModels
{
    /// <summary>
    /// One input channel of the studio: the fixed signal path from the snake over the preamp into the DAW,
    /// e.g. "16CH - 1" -> "UNISON" -> "Apollo x8 - 1". Configured once in the settings - every input list
    /// has one row per studio input.
    /// </summary>
    public class StudioInput : BaseEntity
    {
        /// <summary>Position in the input list (ascending) - the row number.</summary>
        public int SortOrder { get; set; }

        /// <summary>Snake / stage box channel, e.g. "16CH - 1".</summary>
        public string? SnakeChannel { get; set; }

        /// <summary>Preamp (channel), e.g. "UNISON" or "WA412 - 1".</summary>
        public string? Preamp { get; set; }

        /// <summary>DAW / interface input, e.g. "Apollo x8 - 1".</summary>
        public string? DawChannel { get; set; }

        public string? Notes { get; set; }

        public int? InputGroupId { get; set; }
        public InputGroup? InputGroup { get; set; }
    }
}
