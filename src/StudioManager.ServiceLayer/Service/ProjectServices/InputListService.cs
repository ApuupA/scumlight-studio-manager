using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SettingsModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IProjectServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    /// <summary>
    /// Input lists. Only used inputs are stored as rows; every list is returned with one row per studio input
    /// (empty for unused inputs), like the Excel input list.
    /// </summary>
    public class InputListService : BaseService<InputList, InputListReadDto, InputListCreateDto>, IInputListService
    {
        private readonly IRepository<StudioInput> _studioInputRepository;
        private readonly IRepository<InputListRow> _rowRepository;
        private readonly IRepository<ProjectInstrument> _instrumentRepository;

        public InputListService(
            IRepository<InputList> repository,
            IRepository<StudioInput> studioInputRepository,
            IRepository<InputListRow> rowRepository,
            IRepository<ProjectInstrument> instrumentRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _studioInputRepository = studioInputRepository;
            _rowRepository = rowRepository;
            _instrumentRepository = instrumentRepository;
        }

        public override async Task<IEnumerable<InputListReadDto>> GetAll()
        {
            return await LoadWhere(l => true);
        }

        public async Task<IEnumerable<InputListReadDto>> GetForProject(int projectId)
        {
            return await LoadWhere(l => l.ProjectId == projectId);
        }

        public override async Task<InputListReadDto?> Get(int id)
        {
            return (await LoadWhere(l => l.Id == id)).SingleOrDefault();
        }

        public override async Task<InputListReadDto> Insert(InputListCreateDto createDto)
        {
            if (createDto.SortOrder == 0)
            {
                var lists = await _repository.GetAllAsync(l => l.ProjectId == createDto.ProjectId);
                createDto.SortOrder = lists.Select(l => l.SortOrder).DefaultIfEmpty(0).Max() + 1;
            }

            return await base.Insert(createDto);
        }

        public async Task<InputListReadDto> SaveRow(int inputListId, int studioInputId, InputListRowSaveDto dto)
        {
            await EnsureListExists(inputListId);
            if (!await _studioInputRepository.ExistsAsync(i => i.Id == studioInputId))
            {
                throw new EntityNotFoundException(nameof(StudioInput), studioInputId);
            }

            Clean(dto);
            var existing = await _rowRepository.GetAsync(r => r.InputListId == inputListId && r.StudioInputId == studioInputId);

            if (IsEmpty(dto))
            {
                // Nothing plugged in anymore: the input is free again
                if (existing != null)
                {
                    await _rowRepository.DeleteAsync(existing.Id);
                }
            }
            else if (existing != null)
            {
                _mapper.Map(dto, existing);
                await _rowRepository.UpdateAsync(existing);
            }
            else
            {
                var row = _mapper.Map<InputListRow>(dto);
                row.InputListId = inputListId;
                row.StudioInputId = studioInputId;
                await _rowRepository.AddAsync(row);
            }

            return (await Get(inputListId))!;
        }

        public async Task<InputListFillResultDto> FillFromSetups(int inputListId, InputListFillDto dto)
        {
            var list = (await _repository.GetAllWithPathsAsync(l => l.Id == inputListId, "Rows")).SingleOrDefault()
                ?? throw new EntityNotFoundException(nameof(InputList), inputListId);

            var instrumentIds = dto.InstrumentIds.Distinct().ToList();
            var instruments = (await _instrumentRepository.GetAllWithPathsAsync(
                    i => instrumentIds.Contains(i.Id) && i.ProjectId == list.ProjectId,
                    "SetupItems.GearItem", "SetupItems.ExternalGear"))
                .OrderBy(i => i.SortOrder)
                .ToList();

            if (instruments.Count != instrumentIds.Count)
            {
                throw new BusinessRuleException("inputList.instrumentNotInProject", "All instruments must belong to the list's project.");
            }

            // Inputs that are still free, in input order
            var usedInputIds = list.Rows.Select(r => r.StudioInputId).ToHashSet();
            var freeInputs = new Queue<StudioInput>((await _studioInputRepository.GetAllAsync())
                .Where(i => !usedInputIds.Contains(i.Id))
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id));

            var addedCount = 0;
            var notPlacedCount = 0;

            foreach (var instrument in instruments)
            {
                foreach (var item in instrument.SetupItems.Where(s => s.NeedsInput).OrderBy(s => s.SortOrder))
                {
                    // An entry with quantity 2 (e.g. overheads) needs two inputs - skip pieces already in the list
                    var alreadyInList = list.Rows.Count(r => r.SetupItemId == item.Id);

                    for (var piece = alreadyInList + 1; piece <= item.Quantity; piece++)
                    {
                        if (freeInputs.Count == 0)
                        {
                            notPlacedCount++;
                            continue;
                        }

                        var source = item.UsedFor ?? instrument.Name;
                        await _rowRepository.AddAsync(new InputListRow
                        {
                            InputListId = list.Id,
                            StudioInputId = freeInputs.Dequeue().Id,
                            SetupItemId = item.Id,
                            Source = item.Quantity > 1 ? $"{source} {piece}" : source,
                            Microphone = item.GearItem?.Name ?? item.ExternalGear?.Name ?? item.CustomName
                        });
                        addedCount++;
                    }
                }
            }

            return new InputListFillResultDto
            {
                InputList = (await Get(inputListId))!,
                AddedCount = addedCount,
                NotPlacedCount = notPlacedCount
            };
        }

        public async Task<InputListReadDto> ClearRows(int inputListId)
        {
            await EnsureListExists(inputListId);

            var rows = await _rowRepository.GetAllAsync(r => r.InputListId == inputListId);
            await _rowRepository.DeleteRangeAsync(rows);

            return (await Get(inputListId))!;
        }

        /// <summary>
        /// Loads the lists with their rows and builds the complete table: one row per studio input.
        /// </summary>
        private async Task<List<InputListReadDto>> LoadWhere(Expression<Func<InputList, bool>> predicate)
        {
            var lists = await _repository.GetAllWithPathsAsync(predicate, "Rows");
            var studioInputs = (await _studioInputRepository.GetAllAsync(i => i.InputGroup!))
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .ToList();

            return lists
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.Id)
                .Select(list => BuildReadDto(list, studioInputs))
                .ToList();
        }

        private InputListReadDto BuildReadDto(InputList list, List<StudioInput> studioInputs)
        {
            var dto = _mapper.Map<InputListReadDto>(list);
            var rowsByInput = list.Rows.ToDictionary(r => r.StudioInputId);

            dto.Rows = studioInputs.Select((input, index) =>
            {
                var row = rowsByInput.GetValueOrDefault(input.Id);
                return new InputListRowReadDto
                {
                    StudioInputId = input.Id,
                    Number = index + 1,
                    SnakeChannel = input.SnakeChannel,
                    Preamp = input.Preamp,
                    DawChannel = input.DawChannel,
                    InputGroupName = input.InputGroup?.Name,
                    InputGroupColor = input.InputGroup?.Color,
                    RowId = row?.Id,
                    Source = row?.Source,
                    Microphone = row?.Microphone,
                    Stand = row?.Stand,
                    Phantom = row?.Phantom ?? false,
                    Hpf = row?.Hpf ?? false,
                    Tone = row?.Tone ?? false,
                    Eq = row?.Eq ?? false,
                    Insert = row?.Insert,
                    Comment = row?.Comment,
                    SetupItemId = row?.SetupItemId
                };
            }).ToList();

            dto.UsedInputCount = list.Rows.Count;
            return dto;
        }

        private async Task EnsureListExists(int inputListId)
        {
            if (!await _repository.ExistsAsync(l => l.Id == inputListId))
            {
                throw new EntityNotFoundException(nameof(InputList), inputListId);
            }
        }

        /// <summary>Trims the texts, empty texts become null.</summary>
        private static void Clean(InputListRowSaveDto dto)
        {
            static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

            dto.Source = Trim(dto.Source);
            dto.Microphone = Trim(dto.Microphone);
            dto.Stand = Trim(dto.Stand);
            dto.Insert = Trim(dto.Insert);
            dto.Comment = Trim(dto.Comment);
        }

        private static bool IsEmpty(InputListRowSaveDto dto) =>
            dto.Source == null && dto.Microphone == null && dto.Stand == null && dto.Insert == null && dto.Comment == null
            && !dto.Phantom && !dto.Hpf && !dto.Tone && !dto.Eq;
    }
}
