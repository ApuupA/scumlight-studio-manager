namespace StudioManager.ServiceLayer.Results
{
    /// <summary>
    /// A file in the file storage: relative path, size and when it was written (UTC).
    /// </summary>
    public record StoredFileInfo(string RelativePath, long Length, DateTime LastModifiedUtc);
}
