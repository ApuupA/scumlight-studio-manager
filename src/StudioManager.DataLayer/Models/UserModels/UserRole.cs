namespace StudioManager.DataLayer.Models.UserModels
{
    public enum UserRole
    {
        /// <summary>
        /// The studio owner: full access to everything.
        /// </summary>
        Owner,

        /// <summary>
        /// A studio worker: works on the projects (songs, sessions, mixing, ...), but sees nothing of the finance
        /// (times, prices, invoices, quotes, expenses) and can not change the settings or the logins.
        /// </summary>
        Assistant,

        /// <summary>
        /// A band / client: will later only see the projects they are a member of.
        /// </summary>
        Client
    }
}
