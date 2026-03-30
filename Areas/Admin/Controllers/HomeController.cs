using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            // Ví dụ: Thống kê nhanh để hiển thị Dashboard
            ViewBag.TotalOrders = _db.Carts.Count(); // Giả sử dùng đơn hàng
            ViewBag.TotalProducts = _db.Products.Count();
            ViewBag.TotalUsers = _db.Users.Count();

            return View();
        }
    }
}