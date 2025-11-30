using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

public class CustomUserCookieAuthorize : AuthorizeAttribute
{
    protected override bool AuthorizeCore(HttpContextBase context)
    {
        var auth = context.GetOwinContext().Authentication;

        // Lấy identity TỪ UserCookie
        var userIdentity = auth.User;

        // Nếu không có claim AuthenticationType = UserCookie → không cho vào
        var authType = userIdentity?.Identity?.AuthenticationType;

        if (authType != "UserCookie")
            return false;

        return base.AuthorizeCore(context);
    }
}
