using ASP.NET.MVC_NETFramework.Services;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ASP.NET.MVC_NETFramework.Security
{
    public class InternalAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Roles))
            {
                return true;
            }

            var requestedRoles = Roles.Split(',').Select(x => x.Trim());
            return UserRoleService.IsUserInAnyRole(httpContext.User.Identity.Name, requestedRoles);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext?.User;
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                // 403 evita el loop infinito de challenge 401 en Windows Authentication
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
