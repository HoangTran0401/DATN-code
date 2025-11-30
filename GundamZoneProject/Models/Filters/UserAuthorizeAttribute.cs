using System.Web;
using System.Web.Mvc;

public class UserAuthorizeAttribute : AuthorizeAttribute
{
    protected override bool AuthorizeCore(HttpContextBase context)
    {
        var user = context.User;

        if (!user.Identity.IsAuthenticated)
            return false;

        // Chỉ chấp nhận cookie UserCookie
        if (user.Identity.AuthenticationType != "UserCookie")
            return false;

        return true;
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        filterContext.Result = new RedirectResult("/Account/Login");
    }
}
