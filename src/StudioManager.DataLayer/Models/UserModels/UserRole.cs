namespace StudioManager.DataLayer.Models.UserModels
{
    public enum UserRole
    {
        /// <summary>
        /// The studio owner: full access to everything.
        /// </summary>
        Owner,

        /// <summary>
        /// A band / client: will later only see the projects they are a member of.
        /// </summary>
        Client
    }
}
