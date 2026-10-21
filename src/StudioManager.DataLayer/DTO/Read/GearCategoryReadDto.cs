namespace StudioManager.DataLayer.DTO.Read
{
    public class GearCategoryReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsSoftware { get; set; }

        /// <summary>
        /// Number of gear items in this category (shown in the UI, and a category with items can not be deleted).
        /// </summary>
        public int GearItemCount { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
