using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Changes of a quote draft: recipient, date, texts and all lines (the lines are replaced).
    /// </summary>
    public class QuoteUpdateDto
    {
        public DateOnly QuoteDate { get; set; }

        [Required, MaxLength(200)]
        public string RecipientName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? RecipientAddress { get; set; }

        [MaxLength(2000)]
        public string? IntroText { get; set; }

        [MaxLength(2000)]
        public string? ClosingText { get; set; }

        public List<InvoiceLineDto> Lines { get; set; } = new();
    }
}
