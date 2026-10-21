using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Mixing and mastering details of a song, edited on the song page. The API always saves ALL fields.
    /// </summary>
    public class SongMixInfoDto
    {
        // ---------- Mixing: the material from the client ----------

        public DateOnly? MaterialReceivedOn { get; set; }

        /// <summary>Where the multitracks / stems are, e.g. a WeTransfer or Nextcloud link.</summary>
        [Url, MaxLength(2000)]
        public string? MaterialLink { get; set; }

        [Range(0, 1000)]
        public int? MaterialTrackCount { get; set; }

        /// <summary>Problems found, e.g. "Bass clipped in the chorus, kick track missing".</summary>
        [MaxLength(2000)]
        public string? MaterialNotes { get; set; }

        /// <summary>Mix recall: plugin chain, bus settings, ... to repeat the mix later.</summary>
        [MaxLength(8000)]
        public string? MixNotes { get; set; }

        // ---------- Mastering ----------

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
