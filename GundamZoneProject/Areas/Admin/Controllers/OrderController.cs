using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GundamZoneProject.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ================== DANH SÁCH ==================
        public ActionResult Index(int? page)
        {
            var items = db.Orders.OrderByDescending(x => x.CreatedDate).ToList();

            int pageNumber = page ?? 1;
            int pageSize = 10;

            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(items.ToPagedList(pageNumber, pageSize));
        }


        // ================== XEM ĐƠN ==================
        public ActionResult View(int id)
        {
            var item = db.Orders
                .Include("OrderDetails.Product.ProductImage")
                .FirstOrDefault(x => x.Id == id);

            if (item == null)
                return HttpNotFound();

            return View(item);
        }


        // ================== LOAD SẢN PHẨM TRONG ĐƠN ==================
        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails
                .Include("Product.ProductImage")
                .Where(x => x.OrderId == id)
                .ToList();

            return PartialView(items);
        }


        // ================== CẬP NHẬT TRẠNG THÁI (AJAX) ==================
        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            var order = db.Orders
                .Include("OrderDetails.Product")
                .FirstOrDefault(x => x.Id == id);

            if (order == null)
                return Json(new { Success = false });

            int oldStatus = order.Status;

            // ===============================
            // 1. Nếu chuyển từ trạng thái có tính kho → trạng thái KHÔNG tính kho
            // (2 → 1), (2 → 3), (2 → 4)
            // ===============================
            if ((oldStatus == 2 || oldStatus == 3) && trangthai != 2)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = db.Products.Find(detail.ProductId);
                    if (product != null)
                    {
                        product.Quantity += detail.Quantity; // Hoàn kho
                    }
                }
            }

            // ===============================
            // 2. Nếu chuyển từ trạng thái KHÔNG tính kho → trạng thái tính kho
            // (1 → 2), (3 → 2)
            // ===============================
            if ((oldStatus == 1 || oldStatus == 3) && trangthai == 2)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = db.Products.Find(detail.ProductId);
                    if (product != null)
                    {
                        product.Quantity -= detail.Quantity; // Trừ kho
                    }
                }
            }

            // ===============================
            // 3. Cập nhật trạng thái đơn
            // ===============================
            order.Status = trangthai;
            order.ModifiedDate = DateTime.Now;

            db.SaveChanges();

            return Json(new { Success = true });
        }




        // ================== MỞ TRANG EDIT ==================
        public ActionResult Update(int id)
        {
            var item = db.Orders
                .Include("OrderDetails.Product.ProductImage")
                .FirstOrDefault(x => x.Id == id);

            if (item == null)
                return HttpNotFound();

            return View(item);
        }


        // ================== SUBMIT UPDATE ==================
        [HttpPost]
        public ActionResult Update(Order model)
        {
            var order = db.Orders
                .Include("OrderDetails.Product")
                .FirstOrDefault(x => x.Id == model.Id);

            if (order == null)
                return HttpNotFound();

            // Nếu đổi sang HỦY và trước đó không phải HỦY → hoàn kho
            if (model.Status == 4 && order.Status != 4)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = db.Products.Find(detail.ProductId);
                    if (product != null)
                    {
                        product.Quantity += detail.Quantity;
                    }
                }
            }

            order.Status = model.Status;
            order.ModifiedDate = DateTime.Now;

            db.SaveChanges();

            return Redirect("/admin/order/view/" + model.Id);
        }



        // ================== XÓA 1 ĐƠN ==================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var order = db.Orders
                .Include("OrderDetails")
                .FirstOrDefault(x => x.Id == id);

            if (order == null)
                return Json(new { success = false });

            // Xóa chi tiết đơn
            db.OrderDetails.RemoveRange(order.OrderDetails);

            // Xóa đơn
            db.Orders.Remove(order);

            db.SaveChanges();

            return Json(new { success = true });
        }



        // ================== XÓA TẤT CẢ ĐƠN ==================
        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return Json(new { success = false });

            var arr = ids.Split(',').Select(int.Parse).ToList();

            var orders = db.Orders
                .Where(x => arr.Contains(x.Id))
                .Include("OrderDetails")
                .ToList();

            foreach (var order in orders)
            {
                db.OrderDetails.RemoveRange(order.OrderDetails);
                db.Orders.Remove(order);
            }

            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
