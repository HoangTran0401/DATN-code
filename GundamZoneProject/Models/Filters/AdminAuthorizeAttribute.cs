using System.Web;
using System.Web.Mvc;

public class AdminAuthorizeAttribute : AuthorizeAttribute
{
    protected override bool AuthorizeCore(HttpContextBase context)
    {
        var user = context.User;

        if (!user.Identity.IsAuthenticated)
            return false;

        // Chỉ chấp nhận cookie AdminCookie
        if (user.Identity.AuthenticationType != "AdminCookie")
            return false;

        return user.IsInRole("Admin");
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        filterContext.Result = new RedirectResult("/Admin/Account/Login");
    }
}
