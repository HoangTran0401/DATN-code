using GundamZoneProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GundamZoneProject.Controllers
{
    
    public class MenuController : Controller
    {
        public ApplicationDbContext db = new ApplicationDbContext();
        // GET: Menu
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenuTop()
        {
            var items = db.Categories.OrderBy(x => x.Position).ToList();
            return PartialView("_MenuTop", items);  
        }

        public ActionResult MenuProductCategory()
        {
            var items = db.ProductCategories.ToList();
            return PartialView("_MenuProductCategory", items);
        }

        public ActionResult MenuLeft(int? id, string manu)
        {
            if (id != null)
                ViewBag.CateId = id;

            if (!string.IsNullOrEmpty(manu))
                ViewBag.ManuName = manu;

            var categories = db.ProductCategories.ToList();

            var manufacturers = db.Products
                                  .Where(x => x.IsActive && x.Manufacturer != null && x.Manufacturer.Trim() != "")
                                  .Select(x => x.Manufacturer.Trim())
                                  .Distinct()
                                  .OrderBy(x => x)
                                  .ToList();

            ViewBag.Manufacturers = manufacturers;

            return PartialView("_MenuLeft", categories);
        }



        public ActionResult MenuArrivals()
        {
            var items = db.ProductCategories.ToList();
            return PartialView("_MenuArrivals", items);
        }
    }
}