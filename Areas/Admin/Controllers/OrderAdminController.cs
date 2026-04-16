using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào
    public class OrderAdminController : Controller
    {
        private readonly ManaPet _db;

        public OrderAdminController(ManaPet db)
        {
            _db = db;
        }

        // GET: Admin/OrderAdmin/Index
        public async Task<IActionResult> Index()
        {
            var allOrders = await _db.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(allOrders);
        }

        // GET: Admin/OrderAdmin/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Admin/OrderAdmin/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = newStatus;

                // Nếu ông muốn Admin hủy đơn cũng hoàn kho thì thêm đoạn này:
                if (newStatus == "Đã hủy")
                {
                    var details = _db.OrderDetails.Where(d => d.OrderId == id);
                    foreach (var item in details)
                    {
                        var product = await _db.Products.FindAsync(item.ProductId);
                        if (product != null) product.StockQuantity += item.Quantity;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = "Cập nhật đơn hàng #" + id + " thành công!";
            }

            // SỬA DÒNG NÀY ĐỂ RA TRANG DANH SÁCH
            return RedirectToAction(nameof(Index));
        }
    }
}