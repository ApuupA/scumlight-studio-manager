namespace StudioManager.DataLayer.Models.GearModels
{
    /// <summary>
    /// Physical condition / whereabouts of a gear item.
    /// "In use" is intentionally not a status: it will be derived from session bookings (Milestone 2).
    /// </summary>
    public enum GearStatus
    {
        Available,
        Defective,
        InRepair,
        LentOut,
        Retired
    }
}
