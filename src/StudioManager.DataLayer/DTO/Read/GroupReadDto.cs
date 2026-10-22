namespace StudioManager.DataLayer.DTO.Read
{
    public class GroupReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>The members, sorted by name.</summary>
        public List<GroupMemberDto> Members { get; set; } = new();

        /// <summary>How many projects belong to the group.</summary>
        public int ProjectCount { get; set; }
    }
}
