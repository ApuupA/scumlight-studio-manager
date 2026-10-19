using StudioManager.DataLayer.Models;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.IService.IFileServices
{
    /// <summary>
    /// Pictures of entities (gear, clients, bands, projects): checks type and size, stores the file and
    /// keeps PhotoPath of the entity up to date. The services call it with their own repository.
    /// </summary>
    public interface IPhotoService
    {
        /// <summary>
        /// Saves a new picture (JPG, PNG or WEBP, max. 10 MB) and deletes the old one.
        /// </summary>
        /// <param name="folder">Folder in the file storage, e.g. "client-photos".</param>
        Task SetPhoto<T>(IRepository<T> repository, int id, Stream content, string fileName, long length, string folder)
            where T : BaseEntity, IHasPhoto;

        /// <summary>
        /// The picture for downloading, null if the entity has none.
        /// </summary>
        Task<FileDownload?> GetPhoto<T>(IRepository<T> repository, int id) where T : BaseEntity, IHasPhoto;

        /// <summary>
        /// Removes the picture of the entity.
        /// </summary>
        Task DeletePhoto<T>(IRepository<T> repository, int id) where T : BaseEntity, IHasPhoto;

        /// <summary>
        /// Deletes a picture file - used when the entity itself is deleted. Does nothing for null.
        /// </summary>
        void DeleteFile(string? photoPath);
    }
}
