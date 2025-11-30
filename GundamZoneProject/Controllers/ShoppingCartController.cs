using GundamZoneProject;
using GundamZoneProject.Models;
using GundamZoneProject.Models.EntityFrameWork;
using GundamZoneProject.Models.Payments;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace GundamZoneProject.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ShoppingCartController()
        {
        }

        public ShoppingCartController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: ShoppingCart
        [AllowAnonymous]
        public ActionResult Index()
        {

            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult VnpayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                        vnpay.AddResponseData(s, vnpayData[s]);
                }

                string orderCode = vnpay.GetResponseData("vnp_TxnRef");
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                long amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        // cập nhật DB
                        var itemOrder = db.Orders.FirstOrDefault(x => x.Code == orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.Status = 2;
                            db.Entry(itemOrder).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }

                        ViewBag.Status = true;
                        ViewBag.Message = "Thanh toán thành công!";
                        ViewBag.Amount = amount;
                    }
                    else
                    {
                        ViewBag.Status = false;
                        ViewBag.Message = "Thanh toán thất bại hoặc đã huỷ.";

                        var order = db.Orders.FirstOrDefault(x => x.Code == orderCode);
                        if (order != null)
                        {
                            // Lấy chi tiết đơn
                            var details = db.OrderDetails.Where(d => d.OrderId == order.Id).ToList();

                            // Hoàn lại kho
                            foreach (var item in details)
                            {
                                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                                if (product != null)
                                {
                                    product.Quantity += item.Quantity;
                                    db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                                }
                            }

                            // Xóa chi tiết đơn hàng
                            db.OrderDetails.RemoveRange(details);

                            // Xóa đơn hàng
                            db.Orders.Remove(order);

                            db.SaveChanges();
                        }
                    }


                }
                else
                {
                    ViewBag.Status = false;
                    ViewBag.Message = "Sai chữ ký bảo mật!";
                }
            }

            return View();
        }


        [AllowAnonymous]
        public ActionResult CheckOut()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult CheckOutSuccess()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Partial_Item_ThanhToan()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        [AllowAnonymous]
        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }

        [AllowAnonymous]
        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult Partial_CheckOut()
        {
            var user = UserManager.FindByNameAsync(User.Identity.Name).Result;

            var model = new OrderViewModel();

            if (user != null)
            {
                model.CustomerName = user.FullName;
                model.Phone = user.Phone;
                model.Address = user.Address;
                model.Email = user.Email;
            }

            return PartialView(model);
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(OrderViewModel req)
        {
            var code = new { Success = false, Code = -1, Url = "" };

            if (ModelState.IsValid)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart != null)
                {
                    Order order = new Order();
                    order.CustomerName = req.CustomerName;
                    order.Phone = req.Phone;
                    order.Address = req.Address;
                    order.Email = req.Email;
                    order.Status = 1; // 1/Chưa thanh toán

                    cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        Price = x.Price
                    }));

                    // Trừ kho
                    foreach (var cartItem in cart.Items)
                    {
                        var product = db.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                        if (product != null)
                        {
                            product.Quantity -= cartItem.Quantity;
                            if (product.Quantity < 0)
                                product.Quantity = 0;

                            db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    // ===== TÍNH MÃ GIẢM GIÁ =====
                    decimal subTotal = cart.Items.Sum(x => x.Price * x.Quantity);
                    decimal discount = Session["CouponDiscount"] != null ? (decimal)Session["CouponDiscount"] : 0;

                    order.DiscountAmount = discount;
                    order.CouponCode = Session["CouponCode"]?.ToString();
                    order.TotalAmount = subTotal - discount;    // Tổng tiền sau giảm
                                                                // ===== END =====

                    order.TypePayment = req.TypePayment;
                    order.CreatedDate = DateTime.Now;
                    order.ModifiedDate = DateTime.Now;
                    order.CreatedBy = req.Phone;

                    if (User.Identity.IsAuthenticated)
                        order.CustomerId = User.Identity.GetUserId();

                    Random rd = new Random();
                    order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);

                    db.Orders.Add(order);
                    db.SaveChanges();

                    // Gửi mail bạn giữ nguyên như cũ — không xoá gì
                    var strSanPham = "";
                    var thanhtien = decimal.Zero;
                    var TongTien = decimal.Zero;
                    foreach (var sp in cart.Items)
                    {
                        strSanPham += "<tr>";
                        strSanPham += "<td>" + sp.ProductName + "</td>";
                        strSanPham += "<td>" + sp.Quantity + "</td>";
                        strSanPham += "<td>" + GundamZoneProject.Common.Common.FormatNumber(sp.TotalPrice, 0) + "</td>";
                        strSanPham += "</tr>";
                        thanhtien += sp.Price * sp.Quantity;
                    }
                    TongTien = thanhtien;

                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
                    contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                    contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
                    contentCustomer = contentCustomer.Replace("{{Email}}", req.Email);
                    contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentCustomer = contentCustomer.Replace("{{ThanhTien}}", GundamZoneProject.Common.Common.FormatNumber(thanhtien, 0));
                    contentCustomer = contentCustomer.Replace("{{TongTien}}", GundamZoneProject.Common.Common.FormatNumber(TongTien, 0));
                    GundamZoneProject.Common.Common.SendMail("GundamZone", "Đơn hàng #" + order.Code, contentCustomer.ToString(), req.Email);

                    string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
                    contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
                    contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
                    contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
                    contentAdmin = contentAdmin.Replace("{{Email}}", req.Email);
                    contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentAdmin = contentAdmin.Replace("{{ThanhTien}}", GundamZoneProject.Common.Common.FormatNumber(thanhtien, 0));
                    contentAdmin = contentAdmin.Replace("{{TongTien}}", GundamZoneProject.Common.Common.FormatNumber(TongTien, 0));
                    GundamZoneProject.Common.Common.SendMail("GundamZone", "Đơn hàng mới #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);

                    cart.ClearCart();

                    if (req.TypePayment == 2)
                    {
                        var url = UrlPayment(req.TypePaymentVN, order.Code);
                        return Json(new { Success = true, Code = req.TypePayment, Url = url });
                    }

                    return Json(new { Success = true, Code = 1, Url = "" });
                }
            }

            return Json(code);
        }



        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            var db = new ApplicationDbContext();
            var product = db.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
            {
                return Json(new { Success = false, msg = "Sản phẩm không tồn tại!", code = -1 });
            }

            // Giỏ hàng hiện tại
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart == null)
                cart = new ShoppingCart();

            // Tìm xem sản phẩm đã có trong giỏ chưa
            var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == id);

            // Nếu đã có → kiểm tra tổng số lượng
            if (existingItem != null)
            {
                int newTotal = existingItem.Quantity + quantity;

                if (newTotal > product.Quantity)
                {
                    return Json(new
                    {
                        Success = false,
                        msg = "Số lượng trong giỏ vượt quá tồn kho (" + product.Quantity + ")!",
                        code = -1
                    });
                }
            }
            else
            {
                // Nếu chưa có, kiểm tra số lượng mới thêm có vượt tồn không
                if (quantity > product.Quantity)
                {
                    return Json(new
                    {
                        Success = false,
                        msg = "Không thể thêm quá số lượng tồn kho (" + product.Quantity + ")!",
                        code = -1
                    });
                }
            }

            // === Tạo item mới ===
            ShoppingCartItem item = new ShoppingCartItem
            {
                ProductId = product.Id,
                ProductName = product.Title,
                CategoryName = product.ProductCategory.Title,
                Alias = product.Alias,
                Quantity = quantity,
                StockQuantity = product.Quantity,
                ProductImg = product.ProductImage.FirstOrDefault(x => x.IsDefault)?.Image,
                Price = product.PriceSale > 0 ? (decimal)product.PriceSale : product.Price
            };
            item.TotalPrice = item.Quantity * item.Price;

            // === Thêm vào giỏ sau khi đã kiểm tra hợp lệ ===
            cart.AddToCart(item, quantity);
            Session["Cart"] = cart;

            return Json(new { Success = true, msg = "Thêm vào giỏ hàng thành công!", code = 1, Count = cart.Items.Count });
        }



        [HttpPost]
        [AllowAnonymous]
        public ActionResult Update(int id, int quantity)
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (item != null)
                {
                    if (quantity > item.StockQuantity)
                    {
                        return Json(new
                        {
                            Success = false,
                            msg = "Số lượng vượt quá tồn kho (" + item.StockQuantity + ")"
                        });
                    }

                    cart.UpdateQuantity(id, quantity);

                    return Json(new { Success = true });
                }
            }
            return Json(new { Success = false });
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };

            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ApplyCoupon(string coupon)
        {
            if (string.IsNullOrEmpty(coupon))
                return Json(new { success = false, message = "Vui lòng nhập mã!" });

            var cp = db.Coupons.FirstOrDefault(x => x.Code == coupon && x.IsActive);
            if (cp == null)
                return Json(new { success = false, message = "Mã không tồn tại hoặc đã ngừng hoạt động!" });

            if (cp.StartDate.HasValue && cp.StartDate > DateTime.Now)
                return Json(new { success = false, message = "Mã giảm giá chưa bắt đầu!" });

            if (cp.EndDate.HasValue && cp.EndDate < DateTime.Now)
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn!" });

            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart == null || !cart.Items.Any())
                return Json(new { success = false, message = "Giỏ hàng trống!" });

            decimal subTotal = cart.Items.Sum(x => x.Price * x.Quantity);

            if (subTotal < cp.MinOrderValue)
                return Json(new
                {
                    success = false,
                    message = $"Đơn hàng tối thiểu {cp.MinOrderValue:N0} đ!",
                    original = subTotal
                });

            decimal discount = cp.DiscountType == 1
                ? (subTotal * cp.DiscountValue / 100)
                : cp.DiscountValue;

            decimal total = subTotal - discount;

            // Lưu vào Session để Checkout sử dụng
            Session["CouponCode"] = coupon;
            Session["CouponDiscount"] = discount;

            return Json(new
            {
                success = true,
                discount = discount,
                total = total
            });
        }


        #region Thanh toán vnpay
        public string UrlPayment(int TypePaymentVN, string orderCode)
        {
            var urlPayment = "";
            var order = db.Orders.FirstOrDefault(x => x.Code == orderCode);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();
            var Price = (long)order.TotalAmount * 100;
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", Price.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            if (TypePaymentVN == 1)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (TypePaymentVN == 2)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (TypePaymentVN == 3)
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            }

            vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.Code);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.Code); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
            return urlPayment;
        }

        [HttpPost]
        [Authorize]
        public ActionResult CancelOrder(int id)
        {
            var order = db.Orders
                          .Include("OrderDetails")
                          .FirstOrDefault(x => x.Id == id);

            if (order == null)
            {
                return Json(new { Success = false, Message = "Đơn hàng không tồn tại" });
            }

            // Chỉ cho hủy khi Status = 1 hoặc 2
            if (order.Status != 1 && order.Status != 2)
            {
                return Json(new { Success = false, Message = "Đơn hàng không thể hủy!" });
            }

            // 🔥 Hoàn lại số lượng kho
            foreach (var item in order.OrderDetails)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                }
            }

            order.Status = 4; // 4 = Đã hủy
            order.ModifiedDate = DateTime.Now;

            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return Json(new { Success = true });
        }



        #endregion
    }
}