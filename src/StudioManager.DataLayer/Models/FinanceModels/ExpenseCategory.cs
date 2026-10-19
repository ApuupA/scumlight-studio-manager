namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// Category of an expense - the yearly overview sums up the expenses per category.
    /// </summary>
    public enum ExpenseCategory
    {
        /// <summary>Microphones, instruments, computers, furniture, ...</summary>
        Equipment,
        /// <summary>Software and plugins.</summary>
        Software,
        /// <summary>Consumables: strings, cables, tape, drum heads, ...</summary>
        Material,
        /// <summary>External work, e.g. mastering or a session musician.</summary>
        ExternalServices,
        /// <summary>Rent and utilities of the studio room.</summary>
        Rent,
        /// <summary>Advertising, website, merchandise.</summary>
        Marketing,
        Travel,
        /// <summary>Insurances, fees, memberships.</summary>
        Insurance,
        /// <summary>Office supplies, phone, internet.</summary>
        Office,
        Other
    }
}
