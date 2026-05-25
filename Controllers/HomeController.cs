using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Diagnostics;

namespace Petshop_frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ManaPet _context;

        public HomeController(ILogger<HomeController> logger, ManaPet context)
        {
            _logger = logger;
            _context = context;
        }

        #region --- VIEW MODELS / DTOS ---

        public class HomeViewModel
        {
            public List<PetDisplayDto> FeaturedPets { get; set; } = new List<PetDisplayDto>();
            public List<ProductDisplayDto> BestSellingProducts { get; set; } = new List<ProductDisplayDto>();
            public List<BlogDisplayDto> LatestBlogs { get; set; } = new List<BlogDisplayDto>();
        }

        public class PetDisplayDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public decimal? DiscountPrice { get; set; }
            public string Breed { get; set; }
            public int? AgeMonths { get; set; }
            public bool? Gender { get; set; } // Khớp với BIT (1: Đực, 0: Cái)
            public string HealthStatus { get; set; }
            public string ImageUrl { get; set; }
        }

        public class ProductDisplayDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public decimal? DiscountPrice { get; set; }
            public int DiscountPercent { get; set; }
            public string ImageUrl { get; set; }
        }

        public class BlogDisplayDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Slug { get; set; }
            public string ShortDescription { get; set; }
            public string ImageUrl { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        #endregion

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            try
            {
                // 1. Lấy ra 3 bé thú cưng (Lọc đúng theo DB: IsPet = true)
                viewModel.FeaturedPets = await _context.Products
                    .Where(p => p.IsPet == true && p.StockQuantity > 0)
                    .OrderByDescending(p => p.Id)
                    .Take(3)
                    .Select(p => new PetDisplayDto
                    {
                        Id = p.Id,
                        Name = p.ProductName,
                        Price = p.Price ?? 0,
                        DiscountPrice = p.DiscountPrice,
                        Breed = p.FurColor, // DB không có Breed, dùng tạm FurColor hiển thị hoặc đổi tùy ý
                        AgeMonths = p.AgeMonths,
                        Gender = p.Gender,
                        HealthStatus = p.HealthStatus,
                        ImageUrl = p.MainImage
                    })
                    .ToListAsync();

                // 2. Lấy ra 4 sản phẩm phụ kiện (Lọc đúng theo DB: IsPet = false)
                // Vì DB không có cột IsBestSeller, ta sắp xếp theo DiscountPercent hoặc Id để lấy hàng mới
                viewModel.BestSellingProducts = await _context.Products
                    .Where(p => p.IsPet == false && p.StockQuantity > 0)
                    .OrderByDescending(p => p.Id)
                    .Take(4)
                    .Select(p => new ProductDisplayDto
                    {
                        Id = p.Id,
                        Name = p.ProductName,
                        Price = p.Price ?? 0,
                        DiscountPrice = p.DiscountPrice,
                        DiscountPercent = p.DiscountPercent ?? 0, // Xử lý int? sang int
                        ImageUrl = p.MainImage
                    })
                    .ToListAsync();

                // 3. Mở khóa Blog: Lấy ra 3 bài viết tin tức mới nhất từ bảng Blogs của ông giáo
                viewModel.LatestBlogs = await _context.Blogs
                    .Where(b => b.IsPublished == true)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(3)
                    .Select(b => new BlogDisplayDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Slug = b.Slug,
                        ShortDescription = b.ShortDescription,
                        ImageUrl = b.ImageUrl,
                        CreatedAt = b.CreatedAt // Xử lý DATETIME null an toàn
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu trang chủ.");
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}