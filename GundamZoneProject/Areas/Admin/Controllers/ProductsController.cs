using CKFinder.Settings;
using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using PagedList;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace GundamZoneProject.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Products
        public ActionResult Index(string SearchText, int? page)
        {
            var items = db.Products.AsQueryable();

            // Lọc theo từ khóa
            if (!string.IsNullOrEmpty(SearchText))
            {
                items = items.Where(x =>
                    x.Title.Contains(SearchText) ||
                    x.Manufacturer.Contains(SearchText) ||
                    x.ProductCategory.Title.Contains(SearchText)
                );
            }

            int pageSize = 10;
            int pageIndex = page ?? 1;

            ViewBag.SearchText = SearchText;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;

            return View(items.OrderByDescending(x => x.Id).ToPagedList(pageIndex, pageSize));
        }


        public ActionResult Add()
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            var model = new Product
            {
                ProductImage = new List<ProductImage>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++) 
                    {
                        if(i + 1 == rDefault[0])
                        {
                            model.Image = Images[i];
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductID = model.Id,
                                Image = Images[i],
                                IsDefault = true
                            });
                        }
                        else
                        {
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductID = model.Id,
                                Image = Images[i],
                                IsDefault = false
                            });
                        }
                    }
                    
                }
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                if (string.IsNullOrEmpty(model.Alias))
                {
                    model.SeoTitle = model.Title;
                }    
                    if (string.IsNullOrEmpty(model.Alias))    
                    model.Alias = GundamZoneProject.Models.Common.Filter.FilterChar(model.Title);
                db.Products.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var item = db.Products.Find(id);

            ViewBag.ProductCategory = new SelectList(
                db.ProductCategories.ToList(),
                "Id",
                "Title",
                item.ProductCategoryID // Selected Value
            );

            return View(item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = GundamZoneProject.Models.Common.Filter.FilterChar(model.Title);

                db.Products.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            // ======= THÊM DÒNG NÀY =======
            ViewBag.ProductCategory = new SelectList(
                db.ProductCategories.ToList(),
                "Id",
                "Title",
                model.ProductCategoryID
            );

            return View(model);
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                db.Products.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isActive = item.IsActive });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsHome(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.isHome = !item.isHome;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsHome = item.isHome });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsSale(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.isSale = !item.isSale;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsSale = item.isSale });
            }
            return Json(new { success = false });
        }
    }
}