using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ManaPet _db;

        public ProductsController(ManaPet db)
        {
            _db = db;
        }

        public async Task<IActionResult> Cats(string breed, string sortOrder)
        {
            // Mặc định
            breed = string.IsNullOrEmpty(breed) ? "Tất cả" : breed;
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "newest" : sortOrder;

            // CategoryId == 2 (Mèo cảnh) | IsPet == true
            var query = _db.Products
                .Where(p => p.CategoryId == 2 && p.IsPet == true);

            // Xử lý Lọc theo giống
            if (breed != "Tất cả")
            {
                query = query.Where(p => p.Origin.Contains(breed));
            }

            // Xử lý Sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            ViewBag.CurrentBreed = breed;
            ViewBag.CurrentSort = sortOrder;

            var cats = await query.ToListAsync();
            return View(cats);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null || product.CategoryId != 2)
            {
                return NotFound();
            }

            // Lấy 4 bé mèo liên quan (ngẫu nhiên hoặc cùng lứa tuổi) để Upsell
            ViewBag.RelatedCats = await _db.Products
                .Where(p => p.CategoryId == 2 && p.Id != id)
                .OrderBy(r => Guid.NewGuid()) // Random SQL
                .Take(4)
                .ToListAsync();

            return View(product);
        }
        public async Task<IActionResult> Search(string q)
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                string searchLower = q.ToLower();
                query = query.Where(p => 
                    p.ProductName.ToLower().Contains(searchLower) || 
                    p.Origin.ToLower().Contains(searchLower) || 
                    p.HealthStatus.ToLower().Contains(searchLower)
                );
            }

            var results = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            ViewBag.SearchQuery = q;

            // Chuyển kết quả sang View Search để hiển thị
            return View(results);
        }
    }
}
