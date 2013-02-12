using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wikiped.Models
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public new SiteRoles Roles;

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
                base.OnAuthorization(filterContext);
                if (filterContext.HttpContext.Session["KorisnikID"] == null)
                {
                    var url = new UrlHelper(filterContext.RequestContext);
                    var logonUrl = url.Action("LogOn", "SSO");
                    filterContext.Result = new RedirectResult(logonUrl);
                }
                else
                {
                    var url = new UrlHelper(filterContext.RequestContext);
                    var logonUrl = url.Action("LogOFF", "SSO");
                    filterContext.Result = new RedirectResult(logonUrl);
                }
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (Roles != 0)
                return false;

            if (httpContext == null)
                throw new ArgumentNullException("httpContext");
            string[] users = Users.Split(',');
            if (httpContext.Session["KorisnikID"] == null)
            {
                return false;
            }
            else
            {
                return true;
            }


            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            SiteRoles role = (SiteRoles)httpContext.Session["role"];

            if (Roles != 0 && ((Roles & role) != role))
                return false;

            return true;
        }
        [Serializable]
        [Flags]
        public enum SiteRoles
        {
            User = 1 << 0,
            Admin = 1 << 1,
            Helpdesk = 1 << 2
        }
    }
}