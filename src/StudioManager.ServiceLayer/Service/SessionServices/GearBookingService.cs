using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.ISessionServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.SessionServices
{
    /// <summary>
    /// Gear bookings with conflict warnings. Bookings are never blocked - double bookings only produce a warning,
    /// because two sessions on the same day often don't really collide in practice.
    /// </summary>
    public class GearBookingService : BaseService<GearBooking, GearBookingReadDto, GearBookingCreateDto>, IGearBookingService
    {
        private readonly IRepository<Session> _sessionRepository;
        private readonly IRepository<GearItem> _gearItemRepository;

        public GearBookingService(
            IRepository<GearBooking> repository,
            IRepository<Session> sessionRepository,
            IRepository<GearItem> gearItemRepository,
            IMapper mapper) : base(repository, mapper)
        {
            _sessionRepository = sessionRepository;
            _gearItemRepository = gearItemRepository;
        }

        // Session + project for the session info, gear item + category for the gear info
        protected override Expression<Func<GearBooking, object>>[] DefaultIncludes =>
            [b => b.Session!.Project!, b => b.GearItem!.GearCategory!];

        public override async Task<IEnumerable<GearBookingReadDto>> GetAll()
        {
            return await WithWarnings(await _repository.GetAllAsync(DefaultIncludes));
        }

        public override async Task<GearBookingReadDto?> Get(int id)
        {
            var booking = await _repository.GetAsync(b => b.Id == id, DefaultIncludes);
            return booking == null ? null : (await WithWarnings([booking])).Single();
        }

        public async Task<IEnumerable<GearBookingReadDto>> GetForSession(int sessionId)
        {
            var bookings = await _repository.GetAllAsync(b => b.SessionId == sessionId, DefaultIncludes);
            return (await WithWarnings(bookings)).OrderBy(b => b.GearCategoryName).ThenBy(b => b.GearItemName);
        }

        public async Task<IEnumerable<GearBookingReadDto>> GetForProject(int projectId)
        {
            var bookings = await _repository.GetAllAsync(b => b.Session!.ProjectId == projectId, DefaultIncludes);
            return (await WithWarnings(bookings)).OrderBy(b => b.GearCategoryName).ThenBy(b => b.GearItemName);
        }

        public async Task<IEnumerable<GearBookingReadDto>> GetUpcomingForGearItem(int gearItemId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var bookings = await _repository.GetAllAsync(
                b => b.GearItemId == gearItemId && (b.Session!.EndDate ?? b.Session.Date) >= today && b.Session.Status != SessionStatus.Cancelled,
                DefaultIncludes);

            return (await WithWarnings(bookings))
                .OrderBy(b => b.Session.Date)
                .ThenBy(b => b.Session.StartTime ?? TimeOnly.MinValue);
        }

        /// <summary>
        /// Books gear for a session. If the gear is already booked in this session, the quantity is added
        /// to the existing booking instead of creating a second one.
        /// </summary>
        public override async Task<GearBookingReadDto> Insert(GearBookingCreateDto createDto)
        {
            await EnsureSessionAndGearExist(createDto);

            var existing = await _repository.GetAsync(b => b.SessionId == createDto.SessionId && b.GearItemId == createDto.GearItemId);
            if (existing != null)
            {
                existing.Quantity += createDto.Quantity;
                existing.Notes = createDto.Notes ?? existing.Notes;
                await _repository.UpdateAsync(existing);
                return (await Get(existing.Id))!;
            }

            return await base.Insert(createDto);
        }

        public override async Task<GearBookingReadDto> Update(int id, GearBookingCreateDto updateDto)
        {
            await EnsureSessionAndGearExist(updateDto);
            return await base.Update(id, updateDto);
        }

        /// <summary>
        /// True if two sessions take place at the same time: they share at least one day and either one of them
        /// is all-day or their daily time ranges overlap (18:00-22:00 and 22:00-23:00 do not overlap).
        /// </summary>
        public static bool SessionsOverlap(Session a, Session b)
        {
            var shareADay = a.Date <= b.LastDate && b.Date <= a.LastDate;
            if (!shareADay)
            {
                return false;
            }

            if (a.IsAllDay || b.IsAllDay)
            {
                return true;
            }

            return a.StartTime < b.EndTime && b.StartTime < a.EndTime;
        }

        /// <summary>
        /// Maps the bookings to DTOs and fills the warning fields.
        /// The overlap check is simple on purpose: it adds up all bookings of overlapping sessions, even if those
        /// sessions don't overlap with each other - so it rather warns once too often than once too little.
        /// </summary>
        private async Task<List<GearBookingReadDto>> WithWarnings(IEnumerable<GearBooking> bookings)
        {
            var bookingList = bookings.ToList();
            if (bookingList.Count == 0)
            {
                return [];
            }

            // All bookings of the same gear in the same period - one query instead of one per booking
            var gearItemIds = bookingList.Select(b => b.GearItemId).Distinct().ToList();
            var firstDay = bookingList.Min(b => b.Session!.Date);
            var lastDay = bookingList.Max(b => b.Session!.LastDate);
            var sameDayBookings = (await _repository.GetAllAsync(
                b => gearItemIds.Contains(b.GearItemId)
                     && b.Session!.Date <= lastDay
                     && (b.Session.EndDate ?? b.Session.Date) >= firstDay
                     && b.Session.Status != SessionStatus.Cancelled,
                b => b.Session!.Project!)).ToList();

            return bookingList.Select(booking =>
            {
                var dto = _mapper.Map<GearBookingReadDto>(booking);
                dto.TotalBookedQuantity = booking.Quantity;

                // A cancelled session does not block anything, so it gets no warnings either
                if (booking.Session!.Status == SessionStatus.Cancelled)
                {
                    return dto;
                }

                var overlapping = sameDayBookings
                    .Where(other => other.GearItemId == booking.GearItemId
                                    && other.SessionId != booking.SessionId
                                    && SessionsOverlap(other.Session!, booking.Session))
                    .ToList();

                dto.TotalBookedQuantity += overlapping.Sum(other => other.Quantity);
                dto.IsOverbooked = dto.TotalBookedQuantity > dto.AvailableQuantity;
                dto.ConflictingSessions = overlapping
                    .Select(other => _mapper.Map<SessionShortDto>(other.Session))
                    .ToList();

                return dto;
            }).ToList();
        }

        private async Task EnsureSessionAndGearExist(GearBookingCreateDto dto)
        {
            if (!await _sessionRepository.ExistsAsync(s => s.Id == dto.SessionId))
            {
                throw new BusinessRuleException("gearBooking.sessionNotFound", $"Session with id {dto.SessionId} does not exist.");
            }

            if (!await _gearItemRepository.ExistsAsync(g => g.Id == dto.GearItemId))
            {
                throw new BusinessRuleException("gearBooking.gearItemNotFound", $"Gear item with id {dto.GearItemId} does not exist.");
            }
        }
    }
}
