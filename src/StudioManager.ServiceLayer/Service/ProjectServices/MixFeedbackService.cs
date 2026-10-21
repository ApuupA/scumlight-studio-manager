using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class MixFeedbackService : BaseService<MixFeedback, MixFeedbackReadDto, MixFeedbackCreateDto>, IMixFeedbackService
    {
        private readonly IRepository<MixVersion> _versionRepository;
        private readonly IRepository<User> _userRepository;

        public MixFeedbackService(IRepository<MixFeedback> repository, IRepository<MixVersion> versionRepository, IRepository<User> userRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _versionRepository = versionRepository;
            _userRepository = userRepository;
        }

        public async Task<MixFeedbackReadDto> InsertFromStudio(MixFeedbackCreateDto createDto, int userId)
        {
            await Validate(createDto);
            var feedback = _mapper.Map<MixFeedback>(createDto);
            // The name at the moment of writing - stays even if the user is renamed or deleted later
            feedback.Author = (await _userRepository.GetAsync(userId))?.DisplayName;

            await _repository.AddAsync(feedback);
            return (await Get(feedback.Id))!;
        }

        // Load the song, so its title can be shown
        protected override Expression<Func<MixFeedback, object>>[] DefaultIncludes => [f => f.Song!];

        public override async Task<MixFeedbackReadDto> Insert(MixFeedbackCreateDto createDto)
        {
            await Validate(createDto);
            return await base.Insert(createDto);
        }

        public override async Task<MixFeedbackReadDto> Update(int id, MixFeedbackCreateDto updateDto)
        {
            await Validate(updateDto);
            return await base.Update(id, updateDto);
        }

        /// <summary>
        /// Cleans the texts; feedback on a song is only possible if the song is in the version.
        /// </summary>
        private async Task Validate(MixFeedbackCreateDto dto)
        {
            dto.Text = dto.Text.Trim();
            dto.Timecode = string.IsNullOrWhiteSpace(dto.Timecode) ? null : dto.Timecode.Trim();

            var version = (await _versionRepository.GetAllWithPathsAsync(v => v.Id == dto.MixVersionId, "Songs")).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(MixVersion), dto.MixVersionId);

            if (dto.SongId != null && !version.Songs.Any(s => s.Id == dto.SongId))
            {
                throw new BusinessRuleException("mixFeedback.songNotInVersion", "The song is not in this mix version.");
            }
        }
    }
}
