using ASP.NET.MVC_NETFramework.Services;
using System.Linq;
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
    }
}
