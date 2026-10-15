namespace StudioManager.ServiceLayer.Results
{
    /// <summary>
    /// A file to send to the client. The controller returns it with File(...), which also disposes the stream.
    /// </summary>
    public record FileDownload(Stream Content, string ContentType);
}
