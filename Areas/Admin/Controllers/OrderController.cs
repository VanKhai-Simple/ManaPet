using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ManaPet _context;

        public OrderController(ManaPet context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string orderStatus, string paymentStatus)
        {
            var orders = _context.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderCode.Contains(searchString) || 
                                          o.Customer.FullName.Contains(searchString) || 
                                          o.Customer.Phone.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(orderStatus))
            {
                orders = orders.Where(o => o.OrderStatus == orderStatus);
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentStatus == paymentStatus);
            }

            // Thống kê
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Chờ xác nhận");
            ViewBag.ShippingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Đang giao");
            ViewBag.CompletedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Hoàn thành");

            return View(await orders.OrderByDescending(o => o.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Products = _context.Products.Where(p => p.IsPet == true || p.IsPet == false).ToList(); // Lấy tất cả hoặc có thể lọc
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, int[] ProductIds, int[] Quantities, decimal[] Prices, string NewCustomerName, string NewCustomerPhone)
        {
            // Bỏ validation của những trường có thể tự điền/vô hiệu
            ModelState.Remove("OrderCode");
            ModelState.Remove("Customer");
            if (order.CustomerId == 0) ModelState.Remove("CustomerId");

            if (ModelState.IsValid)
            {
                // Nếu chọn Khách hàng mới (CustomerId == 0)
                if (order.CustomerId == 0 && !string.IsNullOrEmpty(NewCustomerName) && !string.IsNullOrEmpty(NewCustomerPhone))
                {
                    var newCustomer = new Customer
                    {
                        FullName = NewCustomerName,
                        Phone = NewCustomerPhone,
                        Address = order.ShippingAddress,
                        CreatedAt = DateTime.Now
                    };
                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();
                    order.CustomerId = newCustomer.Id;
                }
                else if (order.CustomerId == 0)
                {
                    ModelState.AddModelError("CustomerId", "Vui lòng chọn khách hàng hoặc nhập đầy đủ tên và sđt khách mới.");
                    ViewBag.Customers = _context.Customers.ToList();
                    ViewBag.Products = _context.Products.Where(p => p.IsPet == true || p.IsPet == false).ToList();
                    return View(order);
                }

                // Tự sinh mã đơn hàng
                var maxId = await _context.Orders.MaxAsync(o => (int?)o.Id) ?? 0;
                order.OrderCode = "DH" + (maxId + 1).ToString("D3");
                
                order.CreatedAt = DateTime.Now;
                order.UpdatedAt = DateTime.Now;

                _context.Add(order);
                await _context.SaveChangesAsync(); // Lưu để lấy ID
                
                // Lưu OrderDetails
                decimal subtotal = 0;
                for (int i = 0; i < ProductIds.Length; i++)
                {
                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = ProductIds[i],
                        Quantity = Quantities[i],
                        Price = Prices[i],
                        Total = Quantities[i] * Prices[i]
                    };
                    subtotal += detail.Total;
                    _context.Add(detail);
                }
                
                order.Subtotal = subtotal;
                order.TotalAmount = subtotal + order.ShippingFee - order.Discount;
                _context.Update(order);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Products = _context.Products.Where(p => p.IsPet == true || p.IsPet == false).ToList();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string orderStatus, string paymentStatus, string adminNote)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.OrderStatus = orderStatus ?? order.OrderStatus;
            order.PaymentStatus = paymentStatus ?? order.PaymentStatus;
            order.AdminNote = adminNote ?? order.AdminNote;
            order.UpdatedAt = DateTime.Now;

            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id, string cancelReason)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (order.OrderStatus != "Đã hủy")
            {
                order.OrderStatus = "Đã hủy";
                order.AdminNote = (order.AdminNote + " | Lý do hủy: " + cancelReason).Trim(' ', '|');
                order.UpdatedAt = DateTime.Now;

                // Hoàn lại số lượng pet/sản phẩm
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity = (product.StockQuantity ?? 0) + detail.Quantity;
                        _context.Update(product);
                    }
                }

                _context.Update(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
    }
}
