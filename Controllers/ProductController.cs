using Microsoft.AspNetCore.Mvc;
using Petshop_frontend.Models;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Controllers
{
    public class ProductController : Controller
    {
        private readonly ManaPet _context;
        public ProductController(ManaPet context) => _context = context;

        // Trang danh sách (Shop) - Hiển thị các Card
        public async Task<IActionResult> Index(int? categoryId)
        {
            var query = _context.Products.AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            var products = await query.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> ChoCanh()
        {
            // Giả sử ID của danh mục Chó cảnh là 1
            int choCanhId = 1;

            var dsCho = await _context.Products
                .Include(p => p.ProductImages) // Để lấy album ảnh nếu cần
                .Where(p => p.CategoryId == choCanhId && p.IsPet == true)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(dsCho);
        }

        public async Task<IActionResult> Accessories()
        {
            // Lọc những sản phẩm có IsPet = 0 (Phụ kiện)
            var accessories = await _context.Products
                .Where(p => p.IsPet == false)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.CategoryName = "Phụ kiện thú cưng";
            return View(accessories);
        }

        [Route("cho-canh")]
        public async Task<IActionResult> ChoCanh(int? page)
        {
            int pageSize = 12; // 12 card mỗi trang
            int pageNumber = page ?? 1;
            int categoryId = 1; // Giả sử ID Chó cảnh là 1

            var query = _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsPet == true)
                .OrderByDescending(p => p.CreatedAt);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            // Lấy 4 sản phẩm tương tự cùng danh mục (loại trừ chính nó)
            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToListAsync();

            return View(product);
        }
    }
}
