using GundamZoneProject.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PagedList;

namespace GundamZoneProject.Areas.Admin.Controllers
{
    public class ReviewController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // =======================
        // DANH SÁCH ĐÁNH GIÁ (PAGEDLIST)
        // =======================
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var reviews = db.Reviews
                .Include(r => r.Product)
                .OrderByDescending(r => r.Id)   // mới nhất lên đầu
                .ToPagedList(page, pageSize);

            // Gửi dữ liệu phân trang sang View
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(reviews);
        }


        // =======================
        // XÓA ĐÁNH GIÁ
        // =======================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var review = db.Reviews.Find(id);

            if (review == null)
            {
                return Json(new { Success = false, Message = "Không tìm thấy đánh giá." });
            }

            db.Reviews.Remove(review);
            db.SaveChanges();

            return Json(new { Success = true });
        }
    }
}
