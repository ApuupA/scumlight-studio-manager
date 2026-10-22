using AutoMapper;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.UserModels;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService.IUserServices;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service.UserServices
{
    public class GroupService : BaseService<Group, GroupReadDto, GroupCreateDto>, IGroupService
    {
        private readonly IRepository<User> _userRepository;

        public GroupService(IRepository<Group> repository, IRepository<User> userRepository, IMapper mapper) : base(repository, mapper)
        {
            _userRepository = userRepository;
        }

        // Members and projects for the list (names, project count)
        protected override Expression<Func<Group, object>>[] DefaultIncludes => [g => g.Users, g => g.Projects];

        public override async Task<IEnumerable<GroupReadDto>> GetAll()
        {
            return (await base.GetAll()).OrderBy(g => g.Name);
        }

        public override async Task<GroupReadDto> Insert(GroupCreateDto createDto)
        {
            createDto.Name = createDto.Name.Trim();
            await EnsureNameIsFree(createDto.Name, null);

            var group = _mapper.Map<Group>(createDto);
            group.Users = await LoadUsers(createDto.UserIds);

            await _repository.AddAsync(group);
            return (await Get(group.Id))!;
        }

        public override async Task<GroupReadDto> Update(int id, GroupCreateDto updateDto)
        {
            // Load the current members too, so EF knows which links to remove
            var group = await _repository.GetAsync(g => g.Id == id, g => g.Users)
                ?? throw new EntityNotFoundException(nameof(Group), id);

            updateDto.Name = updateDto.Name.Trim();
            await EnsureNameIsFree(updateDto.Name, id);

            _mapper.Map(updateDto, group);
            group.Users.Clear();
            foreach (var user in await LoadUsers(updateDto.UserIds))
            {
                group.Users.Add(user);
            }

            await _repository.UpdateAsync(group);
            return (await Get(id))!;
        }

        private async Task EnsureNameIsFree(string name, int? ownId)
        {
            if (await _repository.ExistsAsync(g => g.Name.ToLower() == name.ToLower() && g.Id != ownId))
            {
                throw new BusinessRuleException("group.nameExists", "Another group already has this name.");
            }
        }

        private async Task<List<User>> LoadUsers(List<int> userIds)
        {
            var ids = userIds.Distinct().ToList();
            var users = (await _userRepository.GetAllAsync(u => ids.Contains(u.Id))).ToList();
            if (users.Count != ids.Count)
            {
                throw new EntityNotFoundException(nameof(User), ids.Except(users.Select(u => u.Id)).First());
            }
            return users;
        }
    }
}
