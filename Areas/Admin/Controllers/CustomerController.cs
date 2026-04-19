using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly ManaPet _context;

        public CustomerController(ManaPet context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Chỉ lấy những ông là Customer để quản lý
            var users = await _context.Users
                .Include(u => u.UserProfile)
                .Where(u => u.Role == "Customer")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Json(new { success = false });

            // Đảo trạng thái 0 <-> 1 (BIT trong SQL là bool trong C#)
            user.Status = !(user.Status ?? true);

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // TRANG CHI TIẾT KHÁCH HÀNG
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Users
                .Include(u => u.UserProfile)
                .Include(u => u.Orders) // Lấy danh sách từ bảng Orders vừa tạo
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }
    }
}