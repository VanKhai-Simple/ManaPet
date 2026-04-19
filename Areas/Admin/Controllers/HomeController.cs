using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ cho phép tài khoản có Role là Admin vào
    public class HomeController : Controller
    {
        private readonly ManaPet _db;

        public HomeController(ManaPet db)
        {
            _db = db;
        }

        //public IActionResult Index()
        //{
        //    // Ví dụ: Thống kê nhanh để hiển thị Dashboard
        //    ViewBag.TotalOrders = _db.Carts.Count(); // Giả sử dùng đơn hàng
        //    ViewBag.TotalProducts = _db.Products.Count();
        //    ViewBag.TotalUsers = _db.Users.Count();

        //    return View();
        //}

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardVM
            {
                NewOrdersCount = await _db.Orders.CountAsync(o => o.Status == "Chờ xác nhận"),
                TotalUsers = await _db.Users.CountAsync(),
                OutOfStockCount = await _db.Products.CountAsync(p => p.StockQuantity <= 0),

                // Lấy 5 đơn hàng mới nhất
                LatestOrders = await _db.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5).ToListAsync(),

                // Lấy 4 thú cưng mới nhập
                LatestProducts = await _db.Products
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4).ToListAsync(),

                RevenueGrowth = 15.5m // Cái này ông có thể viết hàm tính toán sau
            };
            return View(model);
        }

    }
}