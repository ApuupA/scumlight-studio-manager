namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// The secret part of the calendar subscription link (/api/Calendar/{token}.ics). Null = no link created yet.
    /// </summary>
    public class CalendarTokenDto
    {
        public string? Token { get; set; }
    }
}
