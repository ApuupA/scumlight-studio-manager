namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// Just id and name of an instrument, used inside other DTOs (e.g. the instruments of a session).
    /// </summary>
    public class InstrumentShortDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
