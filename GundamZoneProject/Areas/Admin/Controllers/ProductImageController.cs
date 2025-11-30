using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GundamZoneProject.Areas.Admin.Controllers
{
    public class ProductImageController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductImage
        public ActionResult Index(int id)
        {
            ViewBag.ProductId = id;
            var items = db.ProductImages.Where(x => x.ProductID == id).ToList();
            return View(items);
        }

        [HttpPost]
        public ActionResult AddImage(int productId, string url)
        {
            // Kiểm tra sản phẩm đã có ảnh chưa
            bool isDefault = !db.ProductImages.Any(x => x.ProductID == productId);

            var img = new ProductImage
            {
                ProductID = productId,
                Image = url,
                IsDefault = isDefault
            };

            db.ProductImages.Add(img);
            db.SaveChanges();
            return Json(new { Success = true });
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var items = db.ProductImages.Find(id);
            db.ProductImages.Remove(items);
            db.SaveChanges();
            return Json(new {success = true});
        }

        [HttpPost]
        public ActionResult SetDefault(int id)
        {
            var img = db.ProductImages.Find(id);
            if (img != null)
            {
                var all = db.ProductImages.Where(x => x.ProductID == img.ProductID).ToList();
                foreach (var item in all)
                {
                    item.IsDefault = false;
                }

                img.IsDefault = true;
                db.SaveChanges();
            }

            return Json(new { success = true });
        }


    }


}