using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.IService.IFileServices
{
    /// <summary>
    /// Stores files (photos, later documents and mixes). Paths are always relative to the storage root,
    /// so the storage can later be swapped (e.g. to S3) without touching the database or other services.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves the content under a new unique file name in the given folder.
        /// </summary>
        /// <param name="folder">Sub folder, e.g. "gear-photos".</param>
        /// <param name="fileExtension">Extension including the dot, e.g. ".jpg".</param>
        /// <returns>The relative path to store in the database, e.g. "gear-photos/3f2a....jpg".</returns>
        Task<string> SaveAsync(Stream content, string folder, string fileExtension);

        /// <summary>
        /// Opens the file for reading, null if it does not exist. The caller must dispose the stream.
        /// </summary>
        Stream? OpenRead(string relativePath);

        /// <summary>
        /// Deletes the file. Does nothing if it does not exist.
        /// </summary>
        void Delete(string relativePath);

        /// <summary>
        /// Saves the content under a fixed name and replaces an existing file, e.g. "exports/finanzen-2026.zip".
        /// </summary>
        Task WriteAsync(string relativePath, Stream content);

        /// <summary>
        /// The files directly in a folder (no sub folders), empty if the folder does not exist.
        /// </summary>
        IEnumerable<StoredFileInfo> List(string folder);
    }
}
