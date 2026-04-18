using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PetController : Controller
    {
        private readonly ManaPet _db;

        public PetController(ManaPet db)
        {
            _db = db;
        }

        // GET: Admin/Pet
        public async Task<IActionResult> Index()
        {
            // Lọc ra danh sách chỉ chứa Thú Cưng
            var petList = await _db.Products
                                   .Include(p => p.Category)
                                   .Where(p => p.IsPet == true)
                                   .OrderByDescending(p => p.CreatedAt)
                                   .ToListAsync();

            // Thống kê nhanh truyền sang View qua ViewBag
            ViewBag.TotalPets = petList.Count;
            // Mặc định nêú chưa xét Condition thì là Còn bán
            ViewBag.SellingPets = petList.Count(p => p.Condition == "Còn bán" || string.IsNullOrEmpty(p.Condition));
            ViewBag.SoldPets = petList.Count(p => p.Condition == "Đã bán");
            ViewBag.PendingPets = petList.Count(p => p.Condition == "Đang giữ chỗ");
            
            // Có thể định nghĩa thú cưng sắp hết dựa trên Số lượng
            ViewBag.LowStockPets = petList.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 2);

            return View(petList);
        }

        // GET: Admin/Pet/Create
        public IActionResult Create()
        {
            // Dropdown chọn Danh mục (Chó, mèo...)
            ViewBag.CategoryId = new SelectList(_db.Categories, "Id", "CategoryName");
            return View();
        }

        // POST: Admin/Pet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Gán giá trị mặc định cho Thú Cưng
            product.IsPet = true;
            product.CreatedAt = DateTime.Now;

            // Xóa bớt validation của Navigation Properties để tránh lỗi IsValid = false
            ModelState.Remove("Category");
            ModelState.Remove("ProductImages");
            ModelState.Remove("ProductName"); // Nếu form submit thiếu ProductName vẫn có thể bỏ qua để test, nhưng ta nên bắt buộc ở View.

            // Giả sử cứ nhập ProductName thì cho Save
            if (!string.IsNullOrEmpty(product.ProductName))
            {
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Trả lại dữ liệu nếu lỗi
            ViewBag.CategoryId = new SelectList(_db.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Pet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pet = await _db.Products.FindAsync(id);
            if (pet == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_db.Categories, "Id", "CategoryName", pet.CategoryId);
            return View(pet);
        }

        // POST: Admin/Pet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("Category");
            ModelState.Remove("ProductImages");

            if (!string.IsNullOrEmpty(product.ProductName))
            {
                // Giữ lại một số trường không bị update đè bằng db data nếu cần, hoặc để EF lo
                _db.Update(product);
                await _db.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.CategoryId = new SelectList(_db.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // Soft Delete nhanh (Có thể áp dụng sau)
        [HttpPost]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var pet = await _db.Products.FindAsync(id);
            if (pet != null)
            {
                pet.Condition = "Ngừng kinh doanh";
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
