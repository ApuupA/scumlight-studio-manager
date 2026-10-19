namespace StudioManager.DataLayer.Models
{
    /// <summary>
    /// An entity with one picture (gear photo, client / band picture, project cover).
    /// Used by PhotoService, so upload, download and delete are written only once.
    /// </summary>
    public interface IHasPhoto
    {
        /// <summary>
        /// Path of the picture in the file storage, e.g. "client-photos/3f2a....jpg". Null = no picture.
        /// </summary>
        string? PhotoPath { get; set; }
    }
}
