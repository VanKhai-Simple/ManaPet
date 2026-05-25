using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Petshop_frontend.Controllers
{
    public class BlogController : Controller
    {
        private readonly ManaPet _context;

        public BlogController(ManaPet context)
        {
            _context = context;
        }

        // TRANG DANH SÁCH TIN TỨC
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .Where(b => b.IsPublished == true)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(blogs);
        }

        // TRANG CHI TIẾT BÀI VIẾT (Dùng Id cho chắc chắn, hoặc dùng Slug nếu ông giáo làm SEO)
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            // Lấy 3 bài viết khác mới nhất làm bài viết liên quan
            ViewBag.RelatedBlogs = await _context.Blogs
                .Where(b => b.Id != id && b.IsPublished == true)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToListAsync();

            return View(blog);
        }
    }
}