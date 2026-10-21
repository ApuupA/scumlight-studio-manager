namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// An audio sketch for songwriting, e.g. a riff recorded on the phone. Belongs to a project and can be sorted
    /// to a song and to a part of its arrangement ("Riff for verse 2"). Without a song it waits in the inbox of the project.
    /// </summary>
    public class VoiceMemo : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        public string? Notes { get; set; }

        /// <summary>When the memo was recorded (the date of the file on the phone, can be changed).</summary>
        public DateOnly? RecordedOn { get; set; }

        /// <summary>Path of the audio file in the file storage, e.g. "voice-memos/3f2a....m4a".</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>The original file name, e.g. "Neue Aufnahme 37.m4a" - used when saving the file.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>Size of the file in bytes.</summary>
        public long FileSize { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>The song the memo belongs to - null = not sorted yet (inbox).</summary>
        public int? SongId { get; set; }
        public Song? Song { get; set; }

        /// <summary>The part of the song's arrangement, e.g. "Chorus 1" - only together with a song.</summary>
        public int? SongSectionId { get; set; }
        public SongSection? SongSection { get; set; }
    }
}
