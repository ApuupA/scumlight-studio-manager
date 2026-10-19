using StudioManager.DataLayer.Models.FinanceModels;
using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// An expense. The receipt is uploaded separately (POST /api/Expense/{id}/receipt).
    /// </summary>
    public class ExpenseCreateDto
    {
        public DateOnly Date { get; set; }

        [Required, MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Supplier { get; set; }

        public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;

        [Range(0.01, 1000000)]
        public decimal GrossAmount { get; set; }

        [Range(0, 100)]
        public decimal VatRate { get; set; } = 19;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public int? ProjectId { get; set; }
    }
}
