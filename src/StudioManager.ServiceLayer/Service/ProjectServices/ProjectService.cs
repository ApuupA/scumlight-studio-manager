using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;
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

        public const string PhotoFolder = "project-covers";

        public ProjectService(
            IRepository<Project> repository,
            IRepository<Client> clientRepository,
            IRepository<Band> bandRepository,
            IPhotoService photos,
            IRepository<Invoice> invoiceRepository,
            IRepository<Quote> quoteRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _invoiceRepository = invoiceRepository;
            _quoteRepository = quoteRepository;
            _clientRepository = clientRepository;
            _bandRepository = bandRepository;
            _photos = photos;
        }

        // Everything needed for the list: client name, bands and the progress numbers
        protected override Expression<Func<Project, object>>[] DefaultIncludes =>
            [p => p.Client!, p => p.Bands, p => p.Songs, p => p.Sessions];

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

            var project = await _repository.GetAsync(id);
            await base.Delete(id);
            _photos.DeleteFile(project?.PhotoPath);
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
