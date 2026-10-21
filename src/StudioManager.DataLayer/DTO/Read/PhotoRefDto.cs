namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// Points to the picture of another entity, e.g. the client's picture shown for a band without its own.
    /// DateUpdated is only there so the browser reloads the picture after it was replaced.
    /// </summary>
    public class PhotoRefDto
    {
        public int Id { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
