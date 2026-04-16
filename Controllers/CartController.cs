using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Helpers;
using Petshop_frontend.Models;
using System.Security.Claims;

namespace Petshop_frontend.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ManaPet _context;
        private readonly IConfiguration _configuration;

        // Gộp constructor duy nhất ở đây
        public CartController(ManaPet context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        // --- CÁC ACTION GIỎ HÀNG (GIỮ NGUYÊN) ---
        public IActionResult Index()
        {
            int userId = GetCurrentUserId();
            var cartItems = _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.Cart.UserId == userId)
                .ToList();
            return View(cartItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0) return Json(new { success = false, message = "Chưa login" });

                var cartHeader = _context.Carts.FirstOrDefault(c => c.UserId == userId);
                if (cartHeader == null)
                {
                    cartHeader = new Cart { UserId = userId, UpdatedAt = DateTime.Now };
                    _context.Carts.Add(cartHeader);
                    _context.SaveChanges();
                }

                var dbItem = _context.CartItems.FirstOrDefault(ci => ci.CartId == cartHeader.Id && ci.ProductId == productId);
                if (dbItem != null)
                {
                    dbItem.Quantity += quantity;
                    dbItem.AddedDate = DateTime.Now;
                    _context.CartItems.Update(dbItem);
                }
                else
                {
                    dbItem = new CartItem { CartId = cartHeader.Id, ProductId = productId, Quantity = quantity, AddedDate = DateTime.Now };
                    _context.CartItems.Add(dbItem);
                }
                _context.SaveChanges();

                int totalQty = _context.CartItems.Where(ci => ci.CartId == cartHeader.Id).Sum(ci => ci.Quantity);
                return Json(new { success = true, newCount = totalQty });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // --- THANH TOÁN (CHECKOUT) ---
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            int userId = GetCurrentUserId();
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var cartItems = await _context.CartItems.Include(ci => ci.Product).Where(ci => ci.Cart.UserId == userId).ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index");

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalMoney = cartItems.Sum(i => ((i.Product.IsDiscount == true ? i.Product.DiscountPrice : i.Product.Price) ?? 0) * i.Quantity),
                FullName = profile?.FullName,
                PhoneNumber = profile?.Phone,
                Email = profile?.Email,
                Address = profile?.Address
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model, string paymentMethod)
        {
            int userId = GetCurrentUserId();
            var cartItems = _context.CartItems.Include(ci => ci.Product).Where(ci => ci.Cart.UserId == userId).ToList();
            if (!cartItems.Any()) return RedirectToAction("Index");

            // Cập nhật Profile
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                if (string.IsNullOrWhiteSpace(profile.FullName)) profile.FullName = model.FullName;
                if (string.IsNullOrWhiteSpace(profile.Phone)) profile.Phone = model.PhoneNumber;
                if (string.IsNullOrWhiteSpace(profile.Address)) profile.Address = model.Address;
                _context.UserProfiles.Update(profile);
            }

            // Tạo đơn hàng
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Note = model.Note,
                TotalAmount = cartItems.Sum(c => (c.Product?.IsDiscount == true ? c.Product.DiscountPrice : c.Product?.Price) * c.Quantity),
                Status = (paymentMethod == "COD") ? "Chờ xác nhận" : "Chờ thanh toán"
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Lưu chi tiết
            foreach (var item in cartItems)
            {
                var price = item.Product?.IsDiscount == true ? item.Product.DiscountPrice : item.Product?.Price;
                _context.OrderDetails.Add(new OrderDetail { OrderId = order.Id, ProductId = item.ProductId, Price = (decimal)(price ?? 0), Quantity = item.Quantity });
            }
            await _context.SaveChangesAsync();

            // Điều hướng thanh toán
            if (paymentMethod == "VNPAY") return Redirect(GenerateVnPayUrl(order));
            if (paymentMethod == "PAYPAL") return Redirect(GeneratePayPalUrl(order));

            // COD thì xóa giỏ luôn
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return RedirectToAction("Success");
        }

        #region VNPAY
        private string GenerateVnPayUrl(Order order)
        {
            var vnpay = new VnPayLibrary();
            string url = _configuration["VNPAY:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string tmnCode = _configuration["VNPAY:TmnCode"] ?? "YOUR_CODE";
            string hashSecret = _configuration["VNPAY:HashSecret"] ?? "YOUR_SECRET";

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)order.TotalAmount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang #" + order.Id);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", Url.Action("PaymentCallbackVnPay", "Cart", null, Request.Scheme));
            vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

            return vnpay.CreateRequestUrl(url, hashSecret);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnPay()
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in Request.Query)
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_")) vnpay.AddResponseData(key, value);

            string orderIdStr = vnpay.GetResponseData("vnp_TxnRef");
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            bool isValid = vnpay.ValidateSignature(Request.Query["vnp_SecureHash"], _configuration["VNPAY:HashSecret"]);

            if (isValid && vnp_ResponseCode == "00")
            {
                var order = await _context.Orders.FindAsync(int.Parse(orderIdStr));
                if (order != null)
                {
                    order.Status = "Đã thanh toán";
                    _context.CartItems.RemoveRange(_context.CartItems.Where(ci => ci.Cart.UserId == order.UserId));
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Success");
                }
            }
            return Content("Thanh toán thất bại hoặc lỗi chữ ký.");
        }
        #endregion

        #region PAYPAL
        private string GeneratePayPalUrl(Order order)
        {
            double usdAmount = Math.Round((double)(order.TotalAmount ?? 0) / 24500, 2);
            string businessEmail = _configuration["PayPal:Business"] ?? "your-paypal-sandbox@business.com";
            // ReturnURL sẽ dẫn về trang SuccessPayPal để xử lý xóa giỏ hàng
            string returnUrl = Url.Action("SuccessPayPal", "Cart", new { orderId = order.Id }, Request.Scheme);

            return $"https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_xclick&business={businessEmail}&item_name=PetShop_Order_{order.Id}&amount={usdAmount}&currency_code=USD&return={returnUrl}";
        }

        [HttpGet]
        public async Task<IActionResult> SuccessPayPal(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                // Cập nhật trạng thái
                order.Status = "Đã thanh toán (PayPal)";

                // Xóa giỏ hàng của User
                var cartItems = _context.CartItems.Where(ci => ci.Cart.UserId == order.UserId);
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Success");
        }
        #endregion

        public IActionResult Success() => View();

        [HttpPost]
        public IActionResult Remove(int id)
        {
            int userId = GetCurrentUserId();
            var item = _context.CartItems.FirstOrDefault(ci => ci.ProductId == id && ci.Cart.UserId == userId);
            if (item != null) { _context.CartItems.Remove(item); _context.SaveChanges(); }
            return RedirectToAction("Index");
        }


    }
}