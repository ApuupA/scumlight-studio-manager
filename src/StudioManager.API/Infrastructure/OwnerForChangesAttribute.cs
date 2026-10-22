using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudioManager.API.Infrastructure
{
    /// <summary>
    /// Everybody may read, only the owner may change (POST, PUT, DELETE) - for studio settings that assistants need
    /// to work with, e.g. the studio inputs used in the input lists. Assistants get 403 Forbidden for changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OwnerForChangesAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isRead = HttpMethods.IsGet(context.HttpContext.Request.Method) || HttpMethods.IsHead(context.HttpContext.Request.Method);
            if (!isRead && !context.HttpContext.User.IsInRole(Roles.Owner))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
