namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// Just id and name of a band, used inside other DTOs (e.g. the bands of a project).
    /// </summary>
    public class BandShortDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
