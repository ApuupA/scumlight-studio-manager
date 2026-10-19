using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.ServiceLayer.Service.FinanceServices
{
    public class StandardTaskService : BaseService<StandardTask, StandardTaskReadDto, StandardTaskCreateDto>, IStandardTaskService
    {
        private readonly IRepository<TimeEntry> _timeEntryRepository;

        public StandardTaskService(IRepository<StandardTask> repository, IRepository<TimeEntry> timeEntryRepository, IMapper mapper)
            : base(repository, mapper)
        {
            _timeEntryRepository = timeEntryRepository;
        }

        public override async Task<IEnumerable<StandardTaskReadDto>> GetAll()
        {
            var tasks = await base.GetAll();
            return tasks.OrderBy(t => t.SortOrder).ThenBy(t => t.Id);
        }

        public override async Task<StandardTaskReadDto> Insert(StandardTaskCreateDto createDto)
        {
            Clean(createDto);

            if (createDto.SortOrder == 0)
            {
                var tasks = await _repository.GetAllAsync();
                createDto.SortOrder = tasks.Select(t => t.SortOrder).DefaultIfEmpty(0).Max() + 1;
            }

            return await base.Insert(createDto);
        }

        public override async Task<StandardTaskReadDto> Update(int id, StandardTaskCreateDto updateDto)
        {
            Clean(updateDto);
            return await base.Update(id, updateDto);
        }

        public override async Task Delete(int id)
        {
            // Booked time must keep its task - archive it instead, then it is no longer offered
            if (await _timeEntryRepository.ExistsAsync(t => t.StandardTaskId == id))
            {
                throw new BusinessRuleException("standardTask.inUse",
                    "Time is already booked on this task - archive it instead of deleting it.");
            }

            await base.Delete(id);
        }

        /// <summary>
        /// Trims the name; an hour is always one hour.
        /// </summary>
        private static void Clean(StandardTaskCreateDto dto)
        {
            dto.Name = dto.Name.Trim();
            if (dto.Unit == TaskUnit.Hour)
            {
                dto.HoursPerUnit = 1;
            }
        }
    }
}
