namespace StudioManager.DataLayer.DTO.Read
{
    public class PaymentReadDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
