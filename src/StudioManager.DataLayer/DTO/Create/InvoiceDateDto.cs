namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The date for issuing or cancelling - today in the browser's time zone (the server may run in UTC).
    /// </summary>
    public class InvoiceDateDto
    {
        public DateOnly Date { get; set; }
    }
}
