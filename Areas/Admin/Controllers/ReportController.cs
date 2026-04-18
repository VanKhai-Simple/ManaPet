using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportController : Controller
    {
        private readonly ManaPet _context;

        public ReportController(ManaPet context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue) startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!endDate.HasValue) endDate = DateTime.Now;

            // Đảm bảo lấy ranh giới của ngày kết thúc
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);

            // Bảng Orders
            var validOrdersQuery = _context.Orders
                .Where(o => o.OrderDate >= startDate.Value && o.OrderDate <= endOfDay &&
                           (o.OrderStatus == "Hoàn thành" || o.PaymentStatus == "Đã thanh toán"));

            // 1. Các thẻ thống kê
            ViewBag.TotalRevenue = await validOrdersQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            
            ViewBag.TotalExpense = await _context.Expenses
                .Where(e => e.ExpenseDate >= startDate.Value && e.ExpenseDate <= endOfDay)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            ViewBag.Profit = ViewBag.TotalRevenue - ViewBag.TotalExpense;

            ViewBag.CompletedOrders = await _context.Orders
                .CountAsync(o => o.OrderStatus == "Hoàn thành" && o.OrderDate >= startDate.Value && o.OrderDate <= endOfDay);

            // 2. Biểu đồ Doanh thu theo tháng (6 tháng gần nhất)
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.Date.AddMonths(-i))
                .OrderBy(m => m)
                .ToList();

            var monthlyLabels = new List<string>();
            var monthlyRevenue = new List<decimal>();

            foreach (var month in last6Months)
            {
                var firstDay = new DateTime(month.Year, month.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddTicks(-1);
                var sum = await _context.Orders
                    .Where(o => o.OrderDate >= firstDay && o.OrderDate <= lastDay &&
                           (o.OrderStatus == "Hoàn thành" || o.PaymentStatus == "Đã thanh toán"))
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

                monthlyLabels.Add($"T{month.Month}/{month.Year}");
                monthlyRevenue.Add(sum);
            }
            ViewBag.MonthlyLabels = monthlyLabels;
            ViewBag.MonthlyRevenue = monthlyRevenue;

            // 3. Tỷ trọng nhóm sản phẩm (Thú cưng vs Phụ kiện)
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.Order.OrderDate >= startDate.Value && od.Order.OrderDate <= endOfDay &&
                            (od.Order.OrderStatus == "Hoàn thành" || od.Order.PaymentStatus == "Đã thanh toán"))
                .ToListAsync();

            decimal petSales = orderDetails.Where(od => od.Product != null && od.Product.IsPet == true).Sum(od => od.Total);
            decimal accSales = orderDetails.Where(od => od.Product != null && od.Product.IsPet == false).Sum(od => od.Total);

            ViewBag.PetSales = petSales;
            ViewBag.AccessorySales = accSales;

            // 4. Lịch sử giao dịch (Lấy 10 đơn hàng gần nhất và 10 chi phí gần nhất gom lại)
            var recentOrders = await _context.Orders
                .Where(o => o.OrderStatus == "Hoàn thành" || o.PaymentStatus == "Đã thanh toán")
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new TransactionViewModel
                {
                    Code = o.OrderCode,
                    Date = o.OrderDate,
                    Type = "Thu",
                    Description = "Khách mua hàng",
                    Amount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod
                }).ToListAsync();

            var recentExpenses = await _context.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(10)
                .Select(e => new TransactionViewModel
                {
                    Code = e.ExpenseCode,
                    Date = e.ExpenseDate,
                    Type = "Chi",
                    Description = e.ExpenseType,
                    Amount = e.Amount,
                    PaymentMethod = "Trực tiếp"
                }).ToListAsync();

            var transactions = recentOrders.Concat(recentExpenses)
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToList();

            ViewBag.Transactions = transactions;

            // Retain filter dates
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            return View();
        }

        public async Task<IActionResult> Expenses()
        {
            var expenses = await _context.Expenses.OrderByDescending(e => e.ExpenseDate).ToListAsync();
            return View(expenses);
        }

        public IActionResult CreateExpense()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(Expense expense)
        {
            if (ModelState.IsValid)
            {
                var maxId = await _context.Expenses.MaxAsync(e => (int?)e.Id) ?? 0;
                expense.ExpenseCode = "PC" + (maxId + 1).ToString("D4");
                expense.CreatedAt = DateTime.Now;

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Expenses));
            }
            return View(expense);
        }
    }

    public class TransactionViewModel
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // Thu / Chi
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
    }
}
