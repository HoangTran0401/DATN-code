using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using GundamZoneProject.Models;

[assembly: OwinStartupAttribute(typeof(GundamZoneProject.Startup))]
namespace GundamZoneProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateDefaultAdminUser(); // Tạo admin nếu chưa có
        }

        private void CreateDefaultAdminUser()
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Tạo role Admin nếu chưa tồn tại
            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            // Kiểm tra tài khoản admin
            var adminUser = userManager.FindByName("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    FullName = "Administrator"
                };

                // Mật khẩu admin mặc định
                var result = userManager.Create(adminUser, "123456");

                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Admin");
                }
            }
        }
    }
}
