using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class MixFeedbackService : BaseService<MixFeedback, MixFeedbackReadDto, MixFeedbackCreateDto>, IMixFeedbackService
    {
        public MixFeedbackService(IRepository<MixFeedback> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public override async Task<MixFeedbackReadDto> Insert(MixFeedbackCreateDto createDto)
        {
            Clean(createDto);
            return await base.Insert(createDto);
        }

        public override async Task<MixFeedbackReadDto> Update(int id, MixFeedbackCreateDto updateDto)
        {
            Clean(updateDto);
            return await base.Update(id, updateDto);
        }

        private static void Clean(MixFeedbackCreateDto dto)
        {
            dto.Text = dto.Text.Trim();
            dto.Timecode = string.IsNullOrWhiteSpace(dto.Timecode) ? null : dto.Timecode.Trim();
        }
    }
}
