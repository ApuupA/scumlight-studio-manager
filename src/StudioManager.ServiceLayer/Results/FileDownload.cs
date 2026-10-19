namespace StudioManager.ServiceLayer.Results
{
    /// <summary>
    /// A file to send to the client. The controller returns it with File(...), which also disposes the stream.
    /// FileName is the name the browser offers when saving (null for pictures shown on the page).
    /// </summary>
    public record FileDownload(Stream Content, string ContentType, string? FileName = null);
}
