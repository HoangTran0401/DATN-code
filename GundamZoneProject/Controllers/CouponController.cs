using GundamZoneProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GundamZoneProject.Controllers
{
    public class CouponController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        public ActionResult MyCoupons()
        {
            var coupons = db.Coupons
                .Where(x => x.IsActive == true)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            return View(coupons);
        }
    }
}