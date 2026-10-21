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

        /// <summary>
        /// Software and plugins (e.g. NAM, amp sims): can be used in any number of setups at the same time,
        /// so they never count as "in session today".
        /// </summary>
        public bool IsSoftware { get; set; }

        public ICollection<GearItem> GearItems { get; set; } = new List<GearItem>();
    }
}
