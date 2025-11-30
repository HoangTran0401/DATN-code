using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GundamZoneProject.Areas.Admin.Controllers
{
    public class CouponController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(db.Coupons.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Coupon model)
        {
            model.CreatedDate = DateTime.Now;
            db.Coupons.Add(model);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var item = db.Coupons.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(Coupon model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var item = db.Coupons.Find(model.Id);
            if (item == null) return HttpNotFound();

            // ---- UPDATE FIELD TỪ FORM ----
            item.Code = model.Code;
            item.DiscountType = model.DiscountType;
            item.DiscountValue = model.DiscountValue;
            item.MinOrderValue = model.MinOrderValue;
            item.IsActive = model.IsActive;

            // ---- XỬ LÝ NGÀY ----
            if (model.StartDate.HasValue && model.StartDate.Value.Year >= 1900)
                item.StartDate = model.StartDate;
            else
                item.StartDate = null;

            if (model.EndDate.HasValue && model.EndDate.Value.Year >= 1900)
                item.EndDate = model.EndDate;
            else
                item.EndDate = null;

            // ---- LƯU ----
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Coupons.Find(id);
            if (item == null)
                return Json(new { success = false, message = "Không tìm thấy mã giảm giá!" });

            db.Coupons.Remove(item);
            db.SaveChanges();

            return Json(new { success = true });
        }

        public ActionResult IsActive(int id)
        {
            var item = db.Coupons.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isActive = item.IsActive });
            }
            return Json(new { success = false });
        }

    }

}