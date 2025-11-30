using GundamZoneProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GundamZoneProject.Areas.Admin.Controllers
{
    public class StatisticalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Statistical
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetStatistical(string fromDate, string toDate)
        {
            var query = from o in db.Orders
                        join od in db.OrderDetails on o.Id equals od.OrderId
                        join p in db.Products on od.ProductId equals p.Id
                        select new
                        {
                            CreatedDate = o.CreatedDate,
                            Quantity = od.Quantity,
                            Price = od.Price,
                            OriginalPrice = p.OriginalPrice
                        };

            // Lọc ngày bắt đầu
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime startDate = DateTime.Parse(fromDate);
                query = query.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= startDate);
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.Parse(toDate).AddDays(1); // đến ngày
                query = query.Where(x => DbFunctions.TruncateTime(x.CreatedDate) < endDate);
            }


            // Group theo ngày
            var result = query
                .GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate))
                .Select(x => new
                {
                    Date = x.Key,
                    TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
                    TotalSell = x.Sum(y => y.Quantity * y.Price)
                })
                .ToList()
                .Select(x => new
                {
                    Date = x.Date.Value.ToString("dd/MM/yyyy"),
                    DoanhThu = x.TotalSell,
                    LoiNhuan = x.TotalSell - x.TotalBuy
                });

            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }


    }
}