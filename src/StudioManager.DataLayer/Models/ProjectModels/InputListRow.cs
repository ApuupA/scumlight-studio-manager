using StudioManager.DataLayer.Models.SettingsModels;

namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// What is plugged into one studio input in an input list - the columns of the Excel input list
    /// (snake, preamp and DAW channel come from the StudioInput).
    /// </summary>
    public class InputListRow : BaseEntity
    {
        /// <summary>What is recorded, e.g. "Snare Top" or "Room L".</summary>
        public string? Source { get; set; }

        public string? Microphone { get; set; }
        public string? Stand { get; set; }

        /// <summary>Phantom power (+48V).</summary>
        public bool Phantom { get; set; }

        /// <summary>High pass filter of the preamp.</summary>
        public bool Hpf { get; set; }

        /// <summary>"Tone" switch of the preamp (e.g. Unison / WA preamps).</summary>
        public bool Tone { get; set; }

        /// <summary>EQ of the preamp / channel used.</summary>
        public bool Eq { get; set; }

        /// <summary>Insert / outboard, e.g. "1176".</summary>
        public string? Insert { get; set; }

        public string? Comment { get; set; }

        public int InputListId { get; set; }
        public InputList? InputList { get; set; }

        public int StudioInputId { get; set; }
        public StudioInput? StudioInput { get; set; }

        /// <summary>
        /// The setup entry this row was filled from ("fill from setups"), null for rows entered by hand.
        /// </summary>
        public int? SetupItemId { get; set; }
        public InstrumentSetupItem? SetupItem { get; set; }
    }
}
