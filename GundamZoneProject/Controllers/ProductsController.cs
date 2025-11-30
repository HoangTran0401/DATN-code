using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using Microsoft.AspNet.Identity;
using PagedList;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace GundamZoneProject.Controllers
{
    public class ProductsController : Controller
    {
        public ApplicationDbContext db = new ApplicationDbContext();

        // ================================
        // TRANG DANH SÁCH SẢN PHẨM + LỌC + SORT
        // ================================
        public ActionResult Index(int? page, string sort, int? minPrice, int? maxPrice, int? cateId, string manu)
        {
            var items = db.Products
                          .Include(x => x.ProductImage)
                          .Include(x => x.ProductCategory)
                          .Where(x => x.IsActive)
                          .AsQueryable();

            // Lọc theo danh mục
            if (cateId != null)
            {
                items = items.Where(x => x.ProductCategoryID == cateId);

                var cate = db.ProductCategories.Find(cateId);
                if (cate != null)
                {
                    ViewBag.CateName = cate.Title;
                    ViewBag.CateAlias = cate.Alias;
                    ViewBag.CateId = cateId;
                }
            }

            // Lọc theo hãng
            if (!string.IsNullOrEmpty(manu))
            {
                items = items.Where(x => x.Manufacturer == manu);
                ViewBag.ManuName = manu;
            }

            // Lọc theo giá
            // Lọc theo giá
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                items = items.Where(x =>
                    (x.PriceSale > 0 ? x.PriceSale : x.Price) >= minPrice &&
                    (x.PriceSale > 0 ? x.PriceSale : x.Price) <= maxPrice
                );
            }

            // ⭐ BẮT BUỘC: Gửi giá về View để slider không reset
            ViewBag.MinPrice = minPrice ?? 0;
            ViewBag.MaxPrice = maxPrice ?? 2000000;

            // Sắp xếp
            switch (sort)
            {
                case "name_asc": items = items.OrderBy(x => x.Title); break;
                case "name_desc": items = items.OrderByDescending(x => x.Title); break;
                case "price_asc": items = items.OrderBy(x => (x.PriceSale > 0 ? x.PriceSale : x.Price)); break;
                case "price_desc": items = items.OrderByDescending(x => (x.PriceSale > 0 ? x.PriceSale : x.Price)); break;
                case "newest": items = items.OrderByDescending(x => x.CreatedDate); break;
                default: items = items.OrderByDescending(x => x.Id); break;
            }

            int pageSize = 12;
            int pageNumber = page ?? 1;

            return View(items.ToPagedList(pageNumber, pageSize));

        }





        // ================================
        // TRANG CHI TIẾT
        // ================================
        public ActionResult Detail(string alias, int id)
        {
            var item = db.Products
                .Include(x => x.ProductImage)
                .Include(x => x.ProductCategory)
                .FirstOrDefault(x => x.Id == id);

            if (item == null)
                return HttpNotFound();

            // Kiểm tra quyền đánh giá
            ViewBag.CanReview = false;

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();

                ViewBag.CanReview = db.Orders.Any(o =>
                    o.CustomerId == userId &&
                    o.Status == 3 &&
                    o.OrderDetails.Any(d => d.ProductId == id)
                );
            }

            return View(item);
        }


        // ================================
        // TRANG DANH MỤC
        // ================================
        public ActionResult ProductCategory(string alias, int? id, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;

            var items = db.Products
                          .Include(x => x.ProductImage)
                          .Include(x => x.ProductCategory)
                          .Where(x => x.IsActive);

            if (id > 0)
                items = items.Where(x => x.ProductCategoryID == id);

            var result = items.OrderByDescending(x => x.Id)
                              .ToPagedList(pageNumber, pageSize);

            var cate = db.ProductCategories.Find(id);

            ViewBag.CateName = cate?.Title;
            ViewBag.CateAlias = cate?.Alias;
            ViewBag.CateId = id;
            ViewBag.IsCategoryPage = true;

            return View("Index", result);
        }




        // ================================
        // LỌC THEO HÃNG
        // ================================
        public ActionResult Manufacturer(string manu, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;

            var items = db.Products
                .Include(x => x.ProductImage)
                .Include(x => x.ProductCategory)
                .Where(x => x.IsActive);

            if (!string.IsNullOrEmpty(manu))
                items = items.Where(x => x.Manufacturer == manu);

            return View("Index", items.OrderByDescending(x => x.Id)
                .ToPagedList(pageNumber, pageSize));
        }


        // ================================
        // HOME – SẢN PHẨM NỔI BẬT
        // ================================
        public ActionResult Partial_ItemsByCateId()
        {
            var items = db.Products
                .Include(x => x.ProductImage)
                .Where(x => x.isHome && x.IsActive)
                .OrderByDescending(x => x.Id)
                .Take(12)
                .ToList();

            return PartialView(items);
        }


        // ================================
        // HOME – SẢN PHẨM SALE
        // ================================
        public ActionResult Partial_ProductSales()
        {
            var items = db.Products
                .Include(x => x.ProductImage)
                .Where(x => x.isSale && x.IsActive)
                .OrderByDescending(x => x.Id)
                .Take(12)
                .ToList();

            return PartialView(items);
        }


        // ================================
        // HOME – BÁN CHẠY
        // ================================
        public ActionResult Partial_BestSeller()
        {
            var bestSellers = db.OrderDetails
                .GroupBy(x => x.ProductId)
                .Select(g => new BestSellerModel
                {
                    ProductId = g.Key,
                    Alias = g.FirstOrDefault().Product.Alias,
                    Title = g.FirstOrDefault().Product.Title,
                    Image = g.FirstOrDefault().Product.ProductImage.FirstOrDefault(i => i.IsDefault).Image,
                    Price = g.FirstOrDefault().Product.Price,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToList();

            return PartialView(bestSellers);
        }


        // ================================
        // SEARCH
        // ================================
        public ActionResult Search(string keyword, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;

            var items = db.Products
                .Include(x => x.ProductImage)
                .Include(x => x.ProductCategory)
                .Where(x => x.Title.Contains(keyword) || x.Alias.Contains(keyword))
                .OrderByDescending(x => x.Id)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Keyword = keyword;

            return View(items);
        }
    }
}
