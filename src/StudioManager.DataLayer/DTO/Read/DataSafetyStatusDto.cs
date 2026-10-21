namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// State of the data safety: the last backup (written by scripts/backup.sh on the server)
    /// and the finance archives the app writes every night.
    /// </summary>
    public class DataSafetyStatusDto
    {
        /// <summary>When the last backup finished, null if there was none yet (or backup.sh is older).</summary>
        public DateTime? LastBackupAt { get; set; }

        /// <summary>Size of the database dump of the last backup.</summary>
        public long? LastBackupDatabaseBytes { get; set; }

        /// <summary>Where the last backup was copied to off the server (e.g. "hidrive:studio-backups"), null = only on the server.</summary>
        public string? OffsiteTarget { get; set; }

        /// <summary>True if the copy off the server worked, null if there is no off-site target.</summary>
        public bool? OffsiteOk { get; set; }

        /// <summary>The finance archives (one ZIP per year), newest year first.</summary>
        public List<FinanceArchiveDto> Archives { get; set; } = new();
    }

    public class FinanceArchiveDto
    {
        public int Year { get; set; }
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
