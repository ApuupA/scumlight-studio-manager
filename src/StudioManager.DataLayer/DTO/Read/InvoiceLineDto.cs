using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// One invoice line - read and (in the draft) written. Total is calculated by the server.
    /// </summary>
    public class InvoiceLineDto
    {
        [Required, MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Range(-100000, 100000)]
        public decimal Quantity { get; set; }

        [MaxLength(20)]
        public string? Unit { get; set; }

        [Range(-10000000, 10000000)]
        public decimal UnitPrice { get; set; }

        public decimal Total { get; set; }
    }
}
