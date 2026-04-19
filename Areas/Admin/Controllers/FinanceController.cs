using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml; // Cài EPPlus xong mới dùng được cái này
using OfficeOpenXml.Style;
using Petshop_frontend.Models;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được truy cập
    public class FinanceController : Controller
    {
        private readonly ManaPet _context;
        public FinanceController(ManaPet context) { _context = context; }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Orders.AsQueryable();

            // 1. Lọc dữ liệu hiển thị trong Bảng (Vẫn lọc theo OrderDate để xem các đơn phát sinh)
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endOfDay);
            }

            var orders = await query.ToListAsync();

            // 2. Thống kê Cards (Giữ nguyên)
            ViewBag.RealRevenue = orders.Where(o => o.Status == "Đã giao").Sum(o => o.TotalAmount) ?? 0;
            ViewBag.PendingRevenue = orders.Where(o => o.Status == "Chờ xác nhận" || o.Status == "Đang giao").Sum(o => o.TotalAmount) ?? 0;

            // 3. Logic BIỂU ĐỒ: Gom nhóm theo Ngày Giao Thành Công (DeliveredDate)
            // Chỉ lấy những đơn Đã giao và có ngày giao trong khoảng lọc
            var chartData = orders
                .Where(o => o.Status == "Đã giao" && o.DeliveredDate.HasValue)
                .GroupBy(o => o.DeliveredDate.Value.Date) // Gom nhóm theo Ngày (không tính Giờ)
                .Select(g => new {
                    Ngay = g.Key,
                    TongTien = g.Sum(s => s.TotalAmount) ?? 0
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            var monthlyLabels = new List<string>();
            var monthlyRevenue = new List<decimal>();

            foreach (var item in chartData)
            {
                // Hiển thị đầy đủ ngày/tháng để không bị nhầm
                monthlyLabels.Add(item.Ngay.ToString("dd/MM/yyyy"));
                monthlyRevenue.Add(item.TongTien);
            }

            ViewBag.MonthlyLabels = monthlyLabels;
            ViewBag.MonthlyRevenue = monthlyRevenue;

            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }


        public async Task<IActionResult> ExportToExcel()
        {
            var data = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();

            ExcelPackage.License.SetNonCommercialPersonal("Petshop App");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("BaoCaoTaiChinh");

                // Đổ Header
                worksheet.Cells[1, 1].Value = "Mã Đơn";
                worksheet.Cells[1, 2].Value = "Ngày Đặt";
                worksheet.Cells[1, 3].Value = "Khách Hàng";
                worksheet.Cells[1, 4].Value = "Tổng Tiền";
                worksheet.Cells[1, 5].Value = "Trạng Thái";

                // Format Header cho đẹp
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Đổ dữ liệu
                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.Id;
                    worksheet.Cells[row, 2].Value = item.OrderDate.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 3].Value = item.FullName;
                    worksheet.Cells[row, 4].Value = item.TotalAmount;
                    worksheet.Cells[row, 5].Value = item.Status;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                string fileName = $"BaoCao_{DateTime.Now:yyyyMMddHHmm}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}
