using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using System.Web.Mvc;

namespace GundamZoneProject.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        // Form đánh giá
        [AllowAnonymous]
        public ActionResult _Review(int productId)
        {
            ViewBag.ProductId = productId;
            var item = new ReviewProduct();

            if (User.Identity.IsAuthenticated)
            {
                var user = _db.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);
                if (user != null)
                {
                    item.Email = user.Email;
                    item.FullName = user.FullName;
                    item.UserName = user.UserName;
                }
                return PartialView(item);
            }
            return PartialView();
        }

        // Load danh sách đánh giá
        [AllowAnonymous]
        public ActionResult _Load_Review(int productId)
        {
            var item = _db.Reviews
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.Id)
                .ToList();

            ViewBag.Count = item.Count;
            return PartialView(item);
        }

        // Xử lý gửi đánh giá
        [AllowAnonymous]
        [HttpPost]
        public ActionResult PostReview(ReviewProduct req)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { Success = false, Message = "Bạn phải đăng nhập để đánh giá." });
            }

            var userId = User.Identity.GetUserId();

            // Kiểm tra user có mua hàng chưa
            bool didBuy = _db.Orders
                .Any(o => o.CustomerId == userId
                       && o.Status == 2
                       && o.OrderDetails.Any(d => d.ProductId == req.ProductId));

            if (!didBuy)
            {
                return Json(new { Success = false, Message = "Bạn chỉ có thể đánh giá khi đã mua sản phẩm này." });
            }

            // ⭐ Lấy avatar từ bảng tài khoản
            var user = _db.Users.FirstOrDefault(x => x.Id == userId);
            if (user != null)
            {
                req.Avatar = user.AvatarPath;   // ⭐ GÁN AVATAR VÀO REVIEW
                req.FullName = user.FullName;   // Cập nhật fullname chuẩn
                req.Email = user.Email;
            }

            if (ModelState.IsValid)
            {
                req.CreatedDate = DateTime.Now;
                req.UserName = User.Identity.Name;

                _db.Reviews.Add(req);
                _db.SaveChanges();

                return Json(new { Success = true });
            }

            return Json(new { Success = false });
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
