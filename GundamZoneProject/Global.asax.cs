using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using GundamZoneProject.Models;

namespace GundamZoneProject
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.CookieName = "GZ.User.AntiForgery";
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            // ---- FIX LỖI MẤT QUYỀN ADMIN ----
            FixAdminRole();
        }

        private void FixAdminRole()
        {
            var context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // 1. Nếu chưa có role Admin → tạo mới
            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            // 2. Lấy đúng tài khoản admin
            var adminUser = userManager.FindByName("admin");
            if (adminUser == null)
            {
                adminUser = userManager.FindByEmail("admin@gmail.com");
            }

            // 3. Nếu tìm thấy admin → gán lại role Admin
            if (adminUser != null)
            {
                if (!userManager.IsInRole(adminUser.Id, "Admin"))
                {
                    userManager.AddToRole(adminUser.Id, "Admin");
                }
            }

            context.SaveChanges();
        }
    }
}
