using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Text.RegularExpressions;

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

        public class SePayWebhookModel
        {
            public int id { get; set; }
            public string gateway { get; set; } // Ngân hàng nhận
            public DateTime transactionDate { get; set; }
            public string accountName { get; set; }
            public string accountNumber { get; set; }
            public decimal transferAmount { get; set; } // Số tiền chuyển khoản vào
            public string code { get; set; } // Nội dung chuyển khoản thực tế của khách
            public string referenceCode { get; set; } // Mã tham chiếu của ngân hàng (Mã FT...)
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
                if(newStatus == "Đã giao")
                {
                    order.DeliveredDate = DateTime.Now;
                }
                else
                {
                    order.DeliveredDate = null;
                }

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

        [HttpPost]
        [IgnoreAntiforgeryToken] // Bắt buộc phải tắt Token chống giả mạo vì SePay từ ngoài gọi vào
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookModel data)
        {
            if (data == null || string.IsNullOrEmpty(data.code))
            {
                return BadRequest("Dữ liệu trống");
            }

            // 1. Phân tích nội dung chuyển khoản (Lấy chuỗi ký tự khởi đầu bằng DH kèm theo dãy số sau đó)
            // Ví dụ khách chuyển khoản ghi là: "DH1256" hoặc "Chuyen tien don hang DH1256"
            var match = Regex.Match(data.code, @"DH(\d+)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                int orderId = int.Parse(match.Groups[1].Value);

                // 2. Tìm đơn hàng tương ứng trong DB
                var order = await _db.Orders.FindAsync(orderId);
                if (order != null && order.Status == "Chờ thanh toán") // Chỉ xử lý đơn chưa thanh toán
                {
                    // Kiểm tra xem số tiền khách chuyển có khớp với giá trị đơn hàng không
                    if (data.transferAmount >= order.TotalAmount)
                    {
                        // 3. Cập nhật trạng thái đơn hàng thành Đã thanh toán
                        order.Status = "Đã thanh toán";
                        //order.PaymentMethod = "VietQR (SePay Auto)";
                        //order.UpdatedAt = DateTime.Now;

                        _db.Update(order);
                        await _db.SaveChangesAsync();

                        return Ok(new { status = 200, message = "Xác nhận đơn hàng thành công" });
                    }
                }
            }

            return Ok(new { status = 200, message = "Webhook nhận được dữ liệu nhưng không khớp mã đơn" });
        }
    }
}