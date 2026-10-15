using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class SongService : BaseService<Song, SongReadDto, SongCreateDto>, ISongService
    {
        private readonly IRepository<Project> _projectRepository;

        public SongService(IRepository<Song> repository, IRepository<Project> projectRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _projectRepository = projectRepository;
        }

        // Load the band, so BandName can be filled
        protected override Expression<Func<Song, object>>[] DefaultIncludes => [s => s.Band!];

        public async Task<IEnumerable<SongReadDto>> GetForProject(int projectId)
        {
            var songs = await GetAllWhere(s => s.ProjectId == projectId);
            return songs.OrderBy(s => s.TrackNumber).ThenBy(s => s.Title);
        }

        public override async Task<SongReadDto> Insert(SongCreateDto createDto)
        {
            await EnsureBandIsInProject(createDto);
            return await base.Insert(createDto);
        }

        public override async Task<SongReadDto> Update(int id, SongCreateDto updateDto)
        {
            await EnsureBandIsInProject(updateDto);
            return await base.Update(id, updateDto);
        }

        /// <summary>
        /// The project must exist and the song's band (if set) must be one of the project's bands.
        /// </summary>
        private async Task EnsureBandIsInProject(SongCreateDto dto)
        {
            var project = await _projectRepository.GetAsync(p => p.Id == dto.ProjectId, p => p.Bands)
                ?? throw new BusinessRuleException("song.projectNotFound", $"Project with id {dto.ProjectId} does not exist.");

            if (dto.BandId != null && project.Bands.All(b => b.Id != dto.BandId))
            {
                throw new BusinessRuleException("song.bandNotInProject", "The band does not take part in this project.");
            }
        }
    }
}
