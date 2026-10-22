using AutoMapper;
using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.IService.IProjectServices;
using StudioManager.ServiceLayer.Results;
using StudioManager.DataLayer.Models.FinanceModels;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.ProjectServices
{
    public class ProjectService : BaseService<Project, ProjectReadDto, ProjectCreateDto>, IProjectService
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IRepository<Band> _bandRepository;
        private readonly IPhotoService _photos;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IRepository<Quote> _quoteRepository;
        private readonly IRepository<VoiceMemo> _voiceMemoRepository;
        private readonly IFileStorageService _fileStorage;
        private readonly IRepository<Group> _groupRepository;
        private readonly IProjectAccess _access;

        public const string PhotoFolder = "project-covers";

        public ProjectService(
            IRepository<Project> repository,
            IRepository<Client> clientRepository,
            IRepository<Band> bandRepository,
            IPhotoService photos,
            IRepository<Invoice> invoiceRepository,
            IRepository<Quote> quoteRepository,
            IRepository<VoiceMemo> voiceMemoRepository,
            IFileStorageService fileStorage,
            IRepository<Group> groupRepository,
            IProjectAccess access,
            IMapper mapper) : base(repository, mapper)
        {
            _groupRepository = groupRepository;
            _access = access;
            _voiceMemoRepository = voiceMemoRepository;
            _fileStorage = fileStorage;
            _invoiceRepository = invoiceRepository;
            _quoteRepository = quoteRepository;
            _clientRepository = clientRepository;
            _bandRepository = bandRepository;
            _photos = photos;
        }

        // Everything needed for the list: client name, bands and the progress numbers
        protected override Expression<Func<Project, object>>[] DefaultIncludes =>
            [p => p.Client!, p => p.Bands, p => p.Songs, p => p.Sessions, p => p.Groups, p => p.CreatedByUser!];

        public override async Task<IEnumerable<ProjectReadDto>> GetAll()
        {
            var projects = await base.GetAll();
            return projects.OrderByDescending(p => p.DateCreated);
        }

        public async Task<IEnumerable<ProjectReadDto>> GetForClient(int clientId)
        {
            var projects = await GetAllWhere(p => p.ClientId == clientId);
            return projects.OrderByDescending(p => p.DateCreated);
        }

        public override async Task<ProjectReadDto> Insert(ProjectCreateDto createDto)
        {
            await EnsureClientExists(createDto.ClientId);
            var bands = await LoadBandsOfClient(createDto);

            var project = _mapper.Map<Project>(createDto);
            foreach (var band in bands)
            {
                project.Bands.Add(band);
            }

            // An assistant's new project gets their groups - so their team sees it right away
            if (_access.UserId != null)
            {
                var userId = _access.UserId.Value;
                foreach (var group in await _groupRepository.GetAllAsync(g => g.Users.Any(u => u.Id == userId)))
                {
                    project.Groups.Add(group);
                }
            }

            await _repository.AddAsync(project);
            return (await Get(project.Id))!;
        }

        public override async Task<ProjectReadDto> Update(int id, ProjectCreateDto updateDto)
        {
            // Load the current bands too, so EF knows which band links to remove
            var project = await _repository.GetAsync(p => p.Id == id, p => p.Bands)
                ?? throw new EntityNotFoundException(nameof(Project), id);

            await EnsureClientExists(updateDto.ClientId);
            var bands = await LoadBandsOfClient(updateDto);

            _mapper.Map(updateDto, project);
            project.Bands.Clear();
            foreach (var band in bands)
            {
                project.Bands.Add(band);
            }

            await _repository.UpdateAsync(project);
            return (await Get(id))!;
        }

        private async Task EnsureClientExists(int clientId)
        {
            if (!await _clientRepository.ExistsAsync(c => c.Id == clientId))
            {
                throw new BusinessRuleException("project.clientNotFound", $"Client with id {clientId} does not exist.");
            }
        }

        /// <summary>
        /// Loads the bands of the DTO and makes sure all of them belong to the project's client.
        /// </summary>
        private async Task<List<Band>> LoadBandsOfClient(ProjectCreateDto dto)
        {
            var bandIds = dto.BandIds.Distinct().ToList();
            var bands = (await _bandRepository.GetAllAsync(b => bandIds.Contains(b.Id))).ToList();

            if (bands.Count != bandIds.Count || bands.Any(b => b.ClientId != dto.ClientId))
            {
                throw new BusinessRuleException("project.bandNotOfClient",
                    "All bands of a project must belong to the project's client.");
            }

            return bands;
        }

        public override async Task Delete(int id)
        {
            // Invoices must be kept for years - and they belong to the project
            if (await _invoiceRepository.ExistsAsync(i => i.ProjectId == id))
            {
                throw new BusinessRuleException("project.hasInvoices", "The project has invoices and can not be deleted.");
            }

            // Issued quotes are kept as well - drafts are simply deleted with the project
            if (await _quoteRepository.ExistsAsync(q => q.ProjectId == id && q.Number != null))
            {
                throw new BusinessRuleException("project.hasQuotes", "The project has quotes and can not be deleted.");
            }

            var drafts = await _quoteRepository.GetAllAsync(q => q.ProjectId == id);
            foreach (var draft in drafts)
            {
                await _quoteRepository.DeleteAsync(draft.Id);
            }

            // The voice memos are deleted by the database (cascade) - their audio files are deleted here afterwards
            var memoFiles = (await _voiceMemoRepository.GetAllAsync(m => m.ProjectId == id)).Select(m => m.FilePath).ToList();

            var project = await _repository.GetAsync(id);
            await base.Delete(id);
            _photos.DeleteFile(project?.PhotoPath);
            memoFiles.ForEach(_fileStorage.Delete);
        }

        public async Task<ProjectReadDto> SetAccess(int id, ProjectAccessDto dto)
        {
            var project = await _repository.GetAsync(p => p.Id == id, p => p.Groups)
                ?? throw new EntityNotFoundException(nameof(Project), id);

            var groupIds = dto.GroupIds.Distinct().ToList();
            var groups = (await _groupRepository.GetAllAsync(g => groupIds.Contains(g.Id))).ToList();
            if (groups.Count != groupIds.Count)
            {
                throw new EntityNotFoundException(nameof(Group), groupIds.Except(groups.Select(g => g.Id)).First());
            }

            project.IsVisibleToAll = dto.IsVisibleToAll;
            project.Groups.Clear();
            foreach (var group in groups)
            {
                project.Groups.Add(group);
            }

            await _repository.UpdateAsync(project);
            return (await Get(id))!;
        }

        public async Task<ProjectReadDto> SetScope(int id, ProjectScopeDto dto)
        {
            if (!dto.IncludesSongwriting && !dto.IncludesRecording && !dto.IncludesMixing && !dto.IncludesMastering)
            {
                throw new BusinessRuleException("project.scopeEmpty", "At least one of songwriting, recording, mixing and mastering must be on.");
            }

            var project = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(Project), id);
            project.IncludesSongwriting = dto.IncludesSongwriting;
            project.IncludesRecording = dto.IncludesRecording;
            project.IncludesMixing = dto.IncludesMixing;
            project.IncludesMastering = dto.IncludesMastering;
            await _repository.UpdateAsync(project);

            return (await Get(id))!;
        }

        public async Task<ProjectReadDto> SetMaterial(int id, ProjectMaterialDto dto)
        {
            var project = await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(Project), id);
            project.MaterialReceivedOn = dto.MaterialReceivedOn;
            project.MaterialLink = string.IsNullOrWhiteSpace(dto.MaterialLink) ? null : dto.MaterialLink.Trim();
            project.MaterialTrackCount = dto.MaterialTrackCount;
            project.MaterialNotes = string.IsNullOrWhiteSpace(dto.MaterialNotes) ? null : dto.MaterialNotes.Trim();
            await _repository.UpdateAsync(project);

            return (await Get(id))!;
        }

        public async Task<ProjectReadDto> SetPhoto(int id, Stream content, string fileName, long length)
        {
            await _photos.SetPhoto(_repository, id, content, fileName, length, PhotoFolder);
            return (await Get(id))!;
        }

        public async Task<FileDownload?> GetPhoto(int id)
        {
            return await _photos.GetPhoto(_repository, id);
        }

        public async Task<ProjectReadDto> DeletePhoto(int id)
        {
            await _photos.DeletePhoto(_repository, id);
            return (await Get(id))!;
        }
    }
}
