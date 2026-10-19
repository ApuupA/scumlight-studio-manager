using StudioManager.DataLayer.Models;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.Service.FileServices
{
    public class PhotoService : IPhotoService
    {
        public const long MaxPhotoSizeBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Allowed file extensions and the content type the picture is delivered with.
        /// </summary>
        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

        private readonly IFileStorageService _fileStorage;

        public PhotoService(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public async Task SetPhoto<T>(IRepository<T> repository, int id, Stream content, string fileName, long length, string folder)
            where T : BaseEntity, IHasPhoto
        {
            var entity = await GetEntity(repository, id);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!ContentTypes.ContainsKey(extension))
            {
                throw new BusinessRuleException("photo.invalidType", "Only JPG, PNG and WEBP images are allowed.");
            }

            if (length > MaxPhotoSizeBytes)
            {
                throw new BusinessRuleException("photo.tooLarge", "The photo must not be larger than 10 MB.");
            }

            var oldPhotoPath = entity.PhotoPath;

            entity.PhotoPath = await _fileStorage.SaveAsync(content, folder, extension);
            await repository.UpdateAsync(entity);

            // Delete the old file only after the new one is saved successfully
            DeleteFile(oldPhotoPath);
        }

        public async Task<FileDownload?> GetPhoto<T>(IRepository<T> repository, int id) where T : BaseEntity, IHasPhoto
        {
            var entity = await GetEntity(repository, id);
            if (entity.PhotoPath == null)
            {
                return null;
            }

            var stream = _fileStorage.OpenRead(entity.PhotoPath);
            if (stream == null)
            {
                return null;
            }

            var contentType = ContentTypes.GetValueOrDefault(Path.GetExtension(entity.PhotoPath), "application/octet-stream");
            return new FileDownload(stream, contentType);
        }

        public async Task DeletePhoto<T>(IRepository<T> repository, int id) where T : BaseEntity, IHasPhoto
        {
            var entity = await GetEntity(repository, id);
            if (entity.PhotoPath == null)
            {
                return;
            }

            DeleteFile(entity.PhotoPath);
            entity.PhotoPath = null;
            await repository.UpdateAsync(entity);
        }

        public void DeleteFile(string? photoPath)
        {
            if (photoPath != null)
            {
                _fileStorage.Delete(photoPath);
            }
        }

        private static async Task<T> GetEntity<T>(IRepository<T> repository, int id) where T : BaseEntity
        {
            return await repository.GetAsync(id) ?? throw new EntityNotFoundException(typeof(T).Name, id);
        }
    }
}
