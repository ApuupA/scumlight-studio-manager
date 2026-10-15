using StudioManager.ServiceLayer.IService.IFileServices;

namespace StudioManager.ServiceLayer.Service.FileServices
{
    /// <summary>
    /// Stores files on the local disk below a root folder (in Docker this folder is a mounted volume).
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _rootPath;

        public LocalFileStorageService(string rootPath)
        {
            _rootPath = Path.GetFullPath(rootPath);
            Directory.CreateDirectory(_rootPath);
        }

        public async Task<string> SaveAsync(Stream content, string folder, string fileExtension)
        {
            var relativePath = $"{folder}/{Guid.NewGuid():N}{fileExtension.ToLowerInvariant()}";
            var fullPath = ToFullPath(relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            await using var file = File.Create(fullPath);
            await content.CopyToAsync(file);

            return relativePath;
        }

        public Stream? OpenRead(string relativePath)
        {
            var fullPath = ToFullPath(relativePath);
            return File.Exists(fullPath) ? File.OpenRead(fullPath) : null;
        }

        public void Delete(string relativePath)
        {
            var fullPath = ToFullPath(relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        /// <summary>
        /// Converts a relative path into an absolute one and makes sure it stays inside the root folder
        /// (protection against paths like "../../etc/passwd").
        /// </summary>
        private string ToFullPath(string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

            if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Path '{relativePath}' is outside of the storage root.");
            }

            return fullPath;
        }
    }
}
