using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The master values of one song, edited in the album list of the project tab "Mastering".
    /// </summary>
    public class SongMasterInfoDto
    {
        /// <summary>International Standard Recording Code, e.g. "DE-A12-26-00001".</summary>
        [MaxLength(15), RegularExpression(@"^[A-Za-z]{2}-?[A-Za-z0-9]{3}-?\d{2}-?\d{5}$", ErrorMessage = "Invalid ISRC.")]
        public string? Isrc { get; set; }

        /// <summary>Measured loudness of the master in LUFS (integrated).</summary>
        [Range(-60, 0)]
        public decimal? MeasuredLufs { get; set; }

        /// <summary>Measured true peak of the master in dBTP.</summary>
        [Range(-20, 3)]
        public decimal? TruePeak { get; set; }

        /// <summary>Pause before the song on the album in seconds.</summary>
        [Range(0, 60)]
        public decimal? GapBeforeSeconds { get; set; }
    }
}
