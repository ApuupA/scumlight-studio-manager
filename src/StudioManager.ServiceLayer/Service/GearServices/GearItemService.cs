using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IFileServices;
using StudioManager.ServiceLayer.IService.IGearServices;
using StudioManager.ServiceLayer.Results;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.GearServices
{
    public class GearItemService : BaseService<GearItem, GearItemReadDto, GearItemCreateDto>, IGearItemService
    {
        public const string PhotoFolder = "gear-photos";

        private readonly IRepository<GearCategory> _categoryRepository;
        private readonly IRepository<Session> _sessionRepository;
        private readonly IRepository<InstrumentSetupItem> _setupItemRepository;
        private readonly IPhotoService _photos;

        public GearItemService(
            IRepository<GearItem> repository,
            IRepository<GearCategory> categoryRepository,
            IRepository<Session> sessionRepository,
            IRepository<InstrumentSetupItem> setupItemRepository,
            IPhotoService photos,
            IMapper mapper) : base(repository, mapper)
        {
            _categoryRepository = categoryRepository;
            _sessionRepository = sessionRepository;
            _setupItemRepository = setupItemRepository;
            _photos = photos;
        }

        // Load the category as well, so GearCategoryName can be filled
        protected override Expression<Func<GearItem, object>>[] DefaultIncludes => [g => g.GearCategory!];

        public override async Task<IEnumerable<GearItemReadDto>> GetAll()
        {
            var items = (await base.GetAll()).OrderBy(g => g.Name).ToList();
            await SetInSessionToday(items);
            return items;
        }

        public override async Task<GearItemReadDto?> Get(int id)
        {
            var item = await base.Get(id);
            if (item != null)
            {
                await SetInSessionToday([item]);
            }
            return item;
        }

        public override async Task<GearItemReadDto> Insert(GearItemCreateDto createDto)
        {
            await EnsureCategoryExists(createDto.GearCategoryId);
            createDto.GoodFor = CleanTags(createDto.GoodFor);
            return await base.Insert(createDto);
        }

        public override async Task<GearItemReadDto> Update(int id, GearItemCreateDto updateDto)
        {
            await EnsureCategoryExists(updateDto.GearCategoryId);
            updateDto.GoodFor = CleanTags(updateDto.GoodFor);
            return await base.Update(id, updateDto);
        }

        /// <summary>
        /// Trims the "good for" tags and removes empty ones and duplicates ("Snare" and "snare" count as the same).
        /// </summary>
        private static List<string> CleanTags(IEnumerable<string> tags)
        {
            return tags
                .Select(tag => tag.Trim())
                .Where(tag => tag.Length > 0)
                .DistinctBy(tag => tag.ToLowerInvariant())
                .Take(20)
                .ToList();
        }

        public override async Task Delete(int id)
        {
            var item = await GetEntity(id);

            if (await _setupItemRepository.ExistsAsync(s => s.GearItemId == id))
            {
                throw new BusinessRuleException("gearItem.usedInSetups",
                    "The gear is used in the setup of a project. Remove it there first (see 'Used in projects').");
            }

            await base.Delete(id);

            _photos.DeleteFile(item.PhotoPath);
        }

        public async Task<GearItemReadDto> SetPhoto(int id, Stream content, string fileName, long length)
        {
            await _photos.SetPhoto(_repository, id, content, fileName, length, PhotoFolder);
            return (await Get(id))!;
        }

        public async Task<FileDownload?> GetPhoto(int id)
        {
            return await _photos.GetPhoto(_repository, id);
        }

        public async Task<GearItemReadDto> DeletePhoto(int id)
        {
            await _photos.DeletePhoto(_repository, id);
            return (await Get(id))!;
        }

        /// <summary>
        /// Fills InSessionTodayQuantity: pieces used today - from the setups of the instruments recorded
        /// in today's (not cancelled) sessions. Software is left out.
        /// </summary>
        private async Task SetInSessionToday(List<GearItemReadDto> items)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var todaysSessions = await _sessionRepository.GetAllWithPathsAsync(
                s => s.Date <= today && (s.EndDate ?? s.Date) >= today && s.Status != SessionStatus.Cancelled,
                "Instruments.SetupItems.GearItem.GearCategory");

            var quantityPerItem = todaysSessions
                .SelectMany(session => session.Instruments)
                .DistinctBy(instrument => instrument.Id) // an instrument in two sessions today needs its gear only once
                .SelectMany(instrument => instrument.SetupItems)
                .Where(setupItem => setupItem.GearItemId != null)
                // Software can run in any number of setups at the same time
                .Where(setupItem => setupItem.GearItem?.GearCategory?.IsSoftware != true)
                .GroupBy(setupItem => setupItem.GearItemId!.Value)
                .ToDictionary(group => group.Key, group => group.Sum(setupItem => setupItem.Quantity));

            foreach (var item in items)
            {
                item.InSessionTodayQuantity = quantityPerItem.GetValueOrDefault(item.Id);
            }
        }

        private async Task<GearItem> GetEntity(int id)
        {
            return await _repository.GetAsync(id) ?? throw new EntityNotFoundException(nameof(GearItem), id);
        }

        private async Task EnsureCategoryExists(int categoryId)
        {
            if (!await _categoryRepository.ExistsAsync(c => c.Id == categoryId))
            {
                throw new BusinessRuleException("gearItem.categoryNotFound", $"Category with id {categoryId} does not exist.");
            }
        }
    }
}
