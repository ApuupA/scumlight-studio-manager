namespace StudioManager.DataLayer.DTO.Read
{
    public class ProjectCostItemReadDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        /// <summary>Quantity × unit price.</summary>
        public decimal Total { get; set; }

        public bool IsBillable { get; set; }
        public int ProjectId { get; set; }

        /// <summary>On an invoice: its id and number (number null = still a draft).</summary>
        public int? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
    }
}
