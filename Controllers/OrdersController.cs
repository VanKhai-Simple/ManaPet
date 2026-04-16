using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Security.Claims;

namespace Petshop_frontend.Controllers
{
    [Authorize] // Bắt buộc đăng nhập
    public class OrdersController : Controller
    {
        private readonly ManaPet _db;

        public OrdersController(ManaPet db)
        {
            _db = db;
        }

        // GET: Orders/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            // Lấy đúng tên Claim "UserId" như trong UserController của ông
            var userIdClaim = User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "User");
            }

            int userId = int.Parse(userIdClaim);

            var myOrders = await _db.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(myOrders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "User");

            int userId = int.Parse(userIdClaim);

            // Truy vấn đơn hàng kèm chi tiết và thông tin sản phẩm
            // Quan trọng: Kiểm tra UserId == userId để bảo mật (không xem được đơn người khác)
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/CancelOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "User");

            int userId = int.Parse(userIdClaim);

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            // Chỉ cho phép hủy khi đơn hàng đang ở trạng thái Chờ xác nhận
            if (order != null && order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã hủy";

                // Logic hoàn kho (Cộng lại số lượng sản phẩm)
                var details = _db.OrderDetails.Where(d => d.OrderId == id);
                foreach (var item in details)
                {
                    var product = await _db.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = "Đã hủy đơn hàng thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
            }

            return RedirectToAction(nameof(MyOrders));
        }
    }
}