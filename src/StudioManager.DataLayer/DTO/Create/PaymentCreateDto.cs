using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// A received payment for an invoice.
    /// </summary>
    public class PaymentCreateDto
    {
        public DateOnly Date { get; set; }

        [Range(0.01, 10000000)]
        public decimal Amount { get; set; }

        [MaxLength(300)]
        public string? Note { get; set; }
    }
}
