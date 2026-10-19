namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// Status of an invoice as shown in the lists - calculated from InvoiceStatus, payments and due date.
    /// </summary>
    public enum InvoicePaymentStatus
    {
        Draft,
        Open,
        PartiallyPaid,
        Paid,
        Overdue,
        Cancelled,

        /// <summary>A cancellation invoice (negative amount, nothing to pay).</summary>
        Cancellation
    }
}
