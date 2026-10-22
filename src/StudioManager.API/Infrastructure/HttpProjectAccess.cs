using StudioManager.DataLayer.Context;
using System.Security.Claims;

namespace StudioManager.API.Infrastructure
{
    /// <summary>
    /// Who is asking, from the login cookie of the current request (one instance per request).
    /// Without a login (background jobs, public token pages) everything is visible - see IProjectAccess.
    /// </summary>
    public class HttpProjectAccess : IProjectAccess
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>Set by ActAs (calendar subscription) - wins over the cookie.</summary>
        private (int UserId, bool IsOwner)? _actingAs;

        public HttpProjectAccess(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool SeesAllProjects => Current is not { IsOwner: false };

        public int? UserId => SeesAllProjects ? null : Current!.Value.UserId;

        public void ActAs(int userId, bool isOwner)
        {
            _actingAs = (userId, isOwner);
        }

        /// <summary>The user of this request, null without a login.</summary>
        private (int UserId, bool IsOwner)? Current
        {
            get
            {
                if (_actingAs != null)
                {
                    return _actingAs;
                }

                var user = _httpContextAccessor.HttpContext?.User;
                var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user?.Identity?.IsAuthenticated != true || id == null)
                {
                    return null;
                }

                return (int.Parse(id), user.IsInRole(Roles.Owner));
            }
        }
    }
}
