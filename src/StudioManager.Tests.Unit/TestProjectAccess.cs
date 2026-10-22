using StudioManager.DataLayer.Context;

namespace StudioManager.Tests.Unit
{
    /// <summary>
    /// "Who is asking" for tests: the owner by default (sees everything), or AsAssistant(userId).
    /// Pass the same instance to TestHelper.CreateDbContext and to the services.
    /// </summary>
    internal class TestProjectAccess : IProjectAccess
    {
        public bool SeesAllProjects { get; private set; } = true;
        public int? UserId { get; private set; }

        public void ActAs(int userId, bool isOwner)
        {
            SeesAllProjects = isOwner;
            UserId = isOwner ? null : userId;
        }

        public void AsOwner()
        {
            SeesAllProjects = true;
            UserId = null;
        }

        public void AsAssistant(int userId) => ActAs(userId, false);
    }
}
