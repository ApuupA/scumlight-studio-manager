namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// The secret part of a project's band gear link (/band-gear/{token}). Null = no link (or switched off).
    /// </summary>
    public class BandGearLinkDto
    {
        public string? Token { get; set; }
    }
}
