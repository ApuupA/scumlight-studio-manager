using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// An extra item of a project besides the working time, e.g. "2× tape reel" or "Mastering (external)".
    /// </summary>
    public class ProjectCostItem : BaseEntity
    {
        public DateOnly Date { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        /// <summary>False for costs that are not charged to the client.</summary>
        public bool IsBillable { get; set; } = true;

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>The invoice (or draft) this item is on - then it can not be changed anymore.</summary>
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
