using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The mixing notes of one song, edited on the song page.
    /// </summary>
    public class SongMixInfoDto
    {
        /// <summary>Problems found in the material of this song, e.g. "Bass clipped in the chorus".</summary>
        [MaxLength(2000)]
        public string? MaterialNotes { get; set; }

        /// <summary>Mix recall: plugin chain, bus settings, ... to repeat the mix later.</summary>
        [MaxLength(8000)]
        public string? MixNotes { get; set; }
    }
}
