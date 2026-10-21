using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    public interface IProjectService : IBaseService<Project, ProjectReadDto, ProjectCreateDto>
    {
        /// <summary>
        /// All Project entries of one client.
        /// </summary>
        Task<IEnumerable<ProjectReadDto>> GetForClient(int clientId);

        /// <summary>
        /// Uploads a picture (JPG, PNG or WEBP, max. 10 MB) and replaces the existing one.
        /// </summary>
        Task<ProjectReadDto> SetPhoto(int id, Stream content, string fileName, long length);

        /// <summary>
        /// Sets which parts of the work (recording, mixing, mastering) belong to the project. At least one must stay on.
        /// </summary>
        Task<ProjectReadDto> SetScope(int id, ProjectScopeDto dto);

        /// <summary>
        /// Saves the material for the mix: received on, link, number of tracks, notes (tab "Mixing").
        /// </summary>
        Task<ProjectReadDto> SetMaterial(int id, ProjectMaterialDto dto);

        /// <summary>
        /// The picture for downloading, null if there is none.
        /// </summary>
        Task<FileDownload?> GetPhoto(int id);

        /// <summary>
        /// Removes the picture.
        /// </summary>
        Task<ProjectReadDto> DeletePhoto(int id);
    }
}
