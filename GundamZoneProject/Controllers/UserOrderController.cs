using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace WebBanHangOnline.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Lịch sử đơn hàng
        public ActionResult History()
        {
            var userId = User.Identity.GetUserId();   // ID aspnetuser

            var orders = db.Orders
                           .Where(o => o.CustomerId == userId)
                           .OrderByDescending(o => o.CreatedDate)
                           .ToList();

            return View(orders);
        }

        // Chi tiết đơn hàng
        public ActionResult Detail(int id)
        {
            var userId = User.Identity.GetUserId();
            var order = db.Orders
                          .Include("OrderDetails.Product")
                          .FirstOrDefault(o => o.Id == id && o.CustomerId == userId);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }
    }
}
