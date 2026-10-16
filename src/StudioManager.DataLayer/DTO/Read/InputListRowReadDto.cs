namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// One line of an input list: the studio input (fixed signal path) plus what is plugged in.
    /// RowId is null for an input that is not used in this list.
    /// </summary>
    public class InputListRowReadDto
    {
        // ---------- The studio input ----------
        public int StudioInputId { get; set; }

        /// <summary>Running number 1, 2, 3, ... in input order.</summary>
        public int Number { get; set; }

        public string? SnakeChannel { get; set; }
        public string? Preamp { get; set; }
        public string? DawChannel { get; set; }
        public string? InputGroupName { get; set; }
        public string? InputGroupColor { get; set; }

        // ---------- What is plugged in ----------
        public int? RowId { get; set; }
        public string? Source { get; set; }
        public string? Microphone { get; set; }
        public string? Stand { get; set; }
        public bool Phantom { get; set; }
        public bool Hpf { get; set; }
        public bool Tone { get; set; }
        public bool Eq { get; set; }
        public string? Insert { get; set; }
        public string? Comment { get; set; }

        /// <summary>The setup entry the row was filled from, null for rows entered by hand.</summary>
        public int? SetupItemId { get; set; }
    }
}
