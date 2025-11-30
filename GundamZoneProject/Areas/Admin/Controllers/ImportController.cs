using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

namespace GundamZoneProject.Areas.Admin.Controllers
{
    public class ImportController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Import/Create
        public ActionResult Index(string SearchText, int? page)
        {
            var items = db.Products
                .Include("ProductCategory")
                .Include("ProductImage")
                .AsQueryable();

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(SearchText))
            {
                items = items.Where(x =>
                    x.Title.Contains(SearchText) ||
                    x.Manufacturer.Contains(SearchText) ||
                    x.ProductCategory.Title.Contains(SearchText)
                );
            }

            // Phân trang
            int pageSize = 10;
            int pageIndex = page ?? 1;

            ViewBag.SearchText = SearchText;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;

            // 👉 Sắp xếp tồn kho: ít → nhiều
            return View(items.OrderBy(x => x.Quantity).ToPagedList(pageIndex, pageSize));
        }



        // POST: Import/Create
        // POST: Import/Create
        [HttpPost]
        public ActionResult Create(Import import)
        {
            if (ModelState.IsValid)
            {
                var product = db.Products.Find(import.ProductId);

                if (product == null)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm!";
                    return RedirectToAction("Create");
                }

                // 👉 Lưu tồn kho trước khi nhập
                import.OldStock = product.Quantity;

                // 👉 Tính tồn kho sau khi nhập
                import.NewStock = product.Quantity + import.Quantity;

                // 👉 Lưu thời gian nhập
                import.ImportDate = DateTime.Now;

                // 👉 Thêm vào bảng lịch sử
                db.Imports.Add(import);

                // 👉 Cập nhật tồn kho sản phẩm
                product.Quantity = import.NewStock;

                db.SaveChanges();

                TempData["success"] = "Nhập hàng thành công!";
                return RedirectToAction("Create");
            }

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", import.ProductId);
            return View(import);
        }


        // AJAX nhập hàng
        [HttpPost]
        public JsonResult DoImport(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng nhập phải lớn hơn 0" });
            }

            var product = db.Products.Find(productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            // 👉 Lưu lịch sử nhập
            var import = new Import
            {
                ProductId = productId,
                Quantity = quantity,
                OldStock = product.Quantity,
                NewStock = product.Quantity + quantity,
                ImportDate = DateTime.Now
            };

            db.Imports.Add(import);

            // 👉 Cập nhật tồn kho thật
            product.Quantity = import.NewStock;

            db.SaveChanges();

            return Json(new { success = true });
        }


        public ActionResult History(int? page)
        {
            int pageSize = 10;
            int pageIndex = page ?? 1;

            var history = db.Imports
                .Include("Product")
                .OrderByDescending(x => x.ImportDate)
                .ToPagedList(pageIndex, pageSize);   // ✔ TRẢ VỀ PagedList

            return View(history);
        }




    }

}