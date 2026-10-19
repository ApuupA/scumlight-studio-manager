namespace StudioManager.DataLayer.DTO.Read
{
    public class TimeEntryReadDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public decimal Hours { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsBillable { get; set; }

        /// <summary>A running timer (not stopped yet). StartedAt is its start time (UTC).</summary>
        public bool IsRunning { get; set; }
        public DateTime? StartedAt { get; set; }

        public int ProjectId { get; set; }

        /// <summary>On an invoice: its id and number (number null = still a draft).</summary>
        public int? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public int? StandardTaskId { get; set; }
        public string? StandardTaskName { get; set; }
        public int? SessionId { get; set; }
        public string? SessionTitle { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
    }
}
