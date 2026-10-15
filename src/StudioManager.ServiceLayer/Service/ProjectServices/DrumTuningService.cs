using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class DrumTuningService : BaseService<DrumTuning, DrumTuningReadDto, DrumTuningCreateDto>, IDrumTuningService
    {
        public DrumTuningService(IRepository<DrumTuning> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        // Load the song, so SongTitle can be filled
        protected override Expression<Func<DrumTuning, object>>[] DefaultIncludes => [d => d.Song!];

        public async Task<IEnumerable<DrumTuningReadDto>> GetForProject(int projectId)
        {
            var tunings = await GetAllWhere(d => d.ProjectId == projectId);

            // Project-wide tunings first, then per song; inside in the order they were entered (kick, snare, toms, ...)
            return tunings.OrderBy(d => d.SongId != null).ThenBy(d => d.SongTitle).ThenBy(d => d.Id);
        }
    }
}
