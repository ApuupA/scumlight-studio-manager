namespace StudioManager.DataLayer.Models.GearModels
{
    public class GearCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>
        /// Position in lists and dropdowns (ascending).
        /// </summary>
        public int SortOrder { get; set; }

        public ICollection<GearItem> GearItems { get; set; } = new List<GearItem>();
    }
}
