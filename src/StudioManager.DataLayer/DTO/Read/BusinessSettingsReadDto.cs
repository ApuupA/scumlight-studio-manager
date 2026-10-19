namespace StudioManager.DataLayer.DTO.Read
{
    public class BusinessSettingsReadDto
    {
        public decimal DefaultHourlyRate { get; set; }
        public string Currency { get; set; } = "EUR";
    }
}
