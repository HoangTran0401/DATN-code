using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GundamZoneProject.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: News
        public ActionResult Index(int? page)
        {
            int pageSize = 3;
            int pageIndex = page ?? 1;

            var items = db.News
                .Where(x => x.IsActive == true) // CHỈ LẤY TIN CÒN HIỂN THỊ
                .OrderByDescending(x => x.CreatedDate)
                .ToPagedList(pageIndex, pageSize);

            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;

            return View(items);
        }

        public ActionResult Detail(int id)
        {
            var item = db.News.FirstOrDefault(x => x.Id == id && x.IsActive == true); // CHẶN TIN BỊ TẮT

            if (item == null)
            {
                return RedirectToAction("Index"); // hoặc HttpNotFound()
            }

            return View(item);
        }

        public ActionResult Partial_News_Home()
        {
            var items = db.News
                .Where(x => x.IsActive == true) // CHỈ LẤY TIN CÓ HIỂN THỊ
                .OrderByDescending(x => x.CreatedDate)
                .Take(3)
                .ToList();

            return PartialView(items);
        }
    }
}
