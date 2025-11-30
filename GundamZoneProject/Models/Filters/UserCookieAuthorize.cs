using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace GundamZoneProject.Models.CustomAuth
{
    public class UserCookieAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var auth = HttpContext.Current.GetOwinContext().Authentication;
            var userIdentity = auth.AuthenticateAsync("UserCookie").Result?.Identity;

            if (userIdentity == null || !userIdentity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("/Account/Login");
                return;
            }

            HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(userIdentity, null);
        }
    }
}
