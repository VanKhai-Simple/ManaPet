using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class BlogController : Controller
    {
        private readonly ManaPet _context; 

        public BlogController(ManaPet context)
        {
            _context = context;
        }

        // 1. Liệt kê danh sách bài viết
        public async Task<IActionResult> Index()
        {
            return View(await _context.Blogs.OrderByDescending(b => b.CreatedAt).ToListAsync());
        }

        // 2. Trang Thêm mới
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // 3. Trang Chỉnh sửa
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound();
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (id != blog.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Blogs.Any(e => e.Id == blog.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // 4. Xóa bài viết
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}