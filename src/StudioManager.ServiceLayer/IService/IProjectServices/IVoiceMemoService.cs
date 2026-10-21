using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// Voice memos for songwriting: audio sketches of a project, sorted to songs and parts of the arrangement.
    /// </summary>
    public interface IVoiceMemoService : IBaseService<VoiceMemo, VoiceMemoReadDto, VoiceMemoCreateDto>
    {
        /// <summary>The memos of a project, newest recording first.</summary>
        Task<IEnumerable<VoiceMemoReadDto>> GetForProject(int projectId);

        /// <summary>
        /// Saves an uploaded audio file as a new memo. The title is the file name without extension.
        /// </summary>
        /// <param name="songId">Optional: the memo is uploaded on a song page and belongs to that song right away.</param>
        Task<VoiceMemoReadDto> Upload(int projectId, Stream content, string fileName, long length, DateOnly? recordedOn, int? songId);

        /// <summary>The audio file of a memo, null if it is missing.</summary>
        Task<FileDownload?> GetFile(int id);
    }
}
