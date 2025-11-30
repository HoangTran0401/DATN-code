using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;

namespace GundamZoneProject.Areas.Admin.Controllers
{
    public class OverviewController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;

            // ===============================
            // CHỈ LẤY ĐƠN ĐÃ THANH TOÁN (Status = 2)
            // ===============================
            var validOrderIds = db.Orders
                .Where(x => x.Status == 2
                    && x.CreatedDate.Month == month
                    && x.CreatedDate.Year == year)
                .Select(x => x.Id)
                .ToList();

            // ===============================
            // 1. Tổng sản phẩm bán
            // ===============================
            ViewBag.SoldCount = db.OrderDetails
                .Where(x => validOrderIds.Contains(x.OrderId))
                .Sum(x => (int?)x.Quantity) ?? 0;

            // ===============================
            // 2. Tổng đơn đã thanh toán
            // ===============================
            ViewBag.OrderCount = validOrderIds.Count;

            // ===============================
            // 3. Doanh thu
            // ===============================
            ViewBag.Revenue = db.OrderDetails
                .Where(x => validOrderIds.Contains(x.OrderId))
                .Sum(x => (decimal?)(x.Quantity * x.Price)) ?? 0;

            // ===============================
            // 4. SẢN PHẨM BÁN CHẠY
            // ===============================
            var bestSeller = db.OrderDetails
                .Where(x => validOrderIds.Contains(x.OrderId))
                .GroupBy(x => x.ProductId)
                .ToList()
                .Select(g =>
                {
                    var prod = g.First().Product;
                    var defaultImg = prod.ProductImage.FirstOrDefault(i => i.IsDefault);

                    return new Overview
                    {
                        ProductId = prod.Id,
                        Title = prod.Title,
                        Image = defaultImg != null ? defaultImg.Image : "/Content/images/no-image.png",
                        Price = prod.Price,
                        Quantity = g.Sum(x => x.Quantity)
                    };
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToList();

            ViewBag.BestSellers = bestSeller;


            // ===============================
            // 5. SẢN PHẨM BÁN CHẬM
            // ===============================
            var slowSeller = db.Products
                .ToList()
                .Select(p =>
                {
                    var qty = p.OrderDetails
                        ?.Where(od => validOrderIds.Contains(od.OrderId))
                        .Sum(od => (int?)od.Quantity) ?? 0;

                    return new Overview
                    {
                        ProductId = p.Id,
                        Title = p.Title,
                        Image = p.ProductImage.FirstOrDefault(i => i.IsDefault)?.Image
                                ?? "/Content/images/no-image.png",
                        Price = p.Price,
                        Quantity = qty
                    };
                })
                .OrderBy(x => x.Quantity)
                .Take(5)
                .ToList();

            ViewBag.SlowSeller = slowSeller;

            return View();
        }
    }
}
