namespace StudioManager.DataLayer.Context
{
    /// <summary>
    /// Who is asking - decides which projects (and everything that belongs to them) the database returns.
    /// AppDbContext uses it in its query filters, so every query is filtered without the services having to think of it.
    /// Implemented in the API with the login cookie (HttpProjectAccess).
    /// </summary>
    public interface IProjectAccess
    {
        /// <summary>
        /// True for the owner, and for everything without a login: background jobs and the public token pages
        /// (client link, band gear) - the secret token is their key and they only read their own project.
        /// </summary>
        bool SeesAllProjects { get; }

        /// <summary>The logged in user (assistant), null if SeesAllProjects.</summary>
        int? UserId { get; }

        /// <summary>
        /// From now on in this request: act as this user, e.g. for the calendar subscription,
        /// which is opened without login but must only contain the sessions of the token's user.
        /// </summary>
        void ActAs(int userId, bool isOwner);
    }
}
