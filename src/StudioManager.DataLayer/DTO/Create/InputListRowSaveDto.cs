using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The values of one input in an input list. Saving a completely empty row removes it.
    /// </summary>
    public class InputListRowSaveDto
    {
        [MaxLength(100)]
        public string? Source { get; set; }

        [MaxLength(100)]
        public string? Microphone { get; set; }

        [MaxLength(100)]
        public string? Stand { get; set; }

        public bool Phantom { get; set; }
        public bool Hpf { get; set; }
        public bool Tone { get; set; }
        public bool Eq { get; set; }

        [MaxLength(100)]
        public string? Insert { get; set; }

        [MaxLength(200)]
        public string? Comment { get; set; }
    }
}
