using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
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
        public const long MaxPhotoSizeBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Allowed photo file extensions and the content type they are delivered with.
        /// </summary>
        private static readonly Dictionary<string, string> PhotoContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

        private readonly IRepository<GearCategory> _categoryRepository;
        private readonly IRepository<GearBooking> _bookingRepository;
        private readonly IFileStorageService _fileStorage;

        public GearItemService(
            IRepository<GearItem> repository,
            IRepository<GearCategory> categoryRepository,
            IRepository<GearBooking> bookingRepository,
            IFileStorageService fileStorage,
            IMapper mapper) : base(repository, mapper)
        {
            _categoryRepository = categoryRepository;
            _bookingRepository = bookingRepository;
            _fileStorage = fileStorage;
        }

        // Load the category as well, so GearCategoryName can be filled
        protected override Expression<Func<GearItem, object>>[] DefaultIncludes => [g => g.GearCategory!];

        public override async Task<IEnumerable<GearItemReadDto>> GetAll()
        {
            var items = (await base.GetAll()).OrderBy(g => g.Name).ToList();
            await SetBookedToday(items);
            return items;
        }

        public override async Task<GearItemReadDto?> Get(int id)
        {
            var item = await base.Get(id);
            if (item != null)
            {
                await SetBookedToday([item]);
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
            await base.Delete(id);

            if (item.PhotoPath != null)
            {
                _fileStorage.Delete(item.PhotoPath);
            }
        }

        public async Task<GearItemReadDto> SetPhoto(int id, Stream content, string fileName, long length)
        {
            var item = await GetEntity(id);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!PhotoContentTypes.ContainsKey(extension))
            {
                throw new BusinessRuleException("photo.invalidType", "Only JPG, PNG and WEBP images are allowed.");
            }

            if (length > MaxPhotoSizeBytes)
            {
                throw new BusinessRuleException("photo.tooLarge", "The photo must not be larger than 10 MB.");
            }

            var oldPhotoPath = item.PhotoPath;

            item.PhotoPath = await _fileStorage.SaveAsync(content, PhotoFolder, extension);
            await _repository.UpdateAsync(item);

            // Delete the old file only after the new one is saved successfully
            if (oldPhotoPath != null)
            {
                _fileStorage.Delete(oldPhotoPath);
            }

            return (await Get(id))!;
        }

        public async Task<FileDownload?> GetPhoto(int id)
        {
            var item = await GetEntity(id);

            if (item.PhotoPath == null)
            {
                return null;
            }

            var stream = _fileStorage.OpenRead(item.PhotoPath);
            if (stream == null)
            {
                return null;
            }

            var contentType = PhotoContentTypes.GetValueOrDefault(Path.GetExtension(item.PhotoPath), "application/octet-stream");
            return new FileDownload(stream, contentType);
        }

        public async Task<GearItemReadDto> DeletePhoto(int id)
        {
            var item = await GetEntity(id);

            if (item.PhotoPath != null)
            {
                _fileStorage.Delete(item.PhotoPath);
                item.PhotoPath = null;
                await _repository.UpdateAsync(item);
            }

            return (await Get(id))!;
        }

        /// <summary>
        /// Fills BookedTodayQuantity: pieces booked for today's (not cancelled) sessions.
        /// </summary>
        private async Task SetBookedToday(List<GearItemReadDto> items)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var bookings = await _bookingRepository.GetAllAsync(
                b => b.Session!.Date <= today && (b.Session.EndDate ?? b.Session.Date) >= today && b.Session.Status != SessionStatus.Cancelled,
                b => b.Session!);

            var bookedPerItem = bookings
                .GroupBy(b => b.GearItemId)
                .ToDictionary(group => group.Key, group => group.Sum(b => b.Quantity));

            foreach (var item in items)
            {
                item.BookedTodayQuantity = bookedPerItem.GetValueOrDefault(item.Id);
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
