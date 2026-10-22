namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>A member of a group (a login).</summary>
    public class GroupMemberDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}
