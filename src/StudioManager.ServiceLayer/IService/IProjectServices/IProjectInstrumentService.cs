using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// The recorded instruments of a project, each with its setup.
    /// </summary>
    public interface IProjectInstrumentService : IBaseService<ProjectInstrument, ProjectInstrumentReadDto, ProjectInstrumentCreateDto>
    {
        /// <summary>
        /// All instruments of a project with their setup, in the configured order.
        /// </summary>
        Task<IEnumerable<ProjectInstrumentReadDto>> GetForProject(int projectId);

        /// <summary>
        /// Creates a new instrument in the project with the setup of a template.
        /// </summary>
        Task<ProjectInstrumentReadDto> ApplyTemplate(ApplySetupTemplateDto dto);
    }
}
