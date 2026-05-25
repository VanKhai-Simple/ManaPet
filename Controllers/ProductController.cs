using Microsoft.AspNetCore.Mvc;
using Petshop_frontend.Models;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Controllers
{
    public class ProductController : Controller
    {
        private readonly ManaPet _context;
        public ProductController(ManaPet context) => _context = context;

        // 1. Trang danh sách tổng (Shop)
        // 1. Trang danh sách tổng (Shop) - Đã thêm lọc giá và phân trang 32 sản phẩm/trang
        public async Task<IActionResult> Index(int? categoryId, string priceRange, int? page)
        {
            int pageSize = 32; // Hiển thị tối đa 32 sản phẩm trên 1 trang
            int pageNumber = page ?? 1;

            var query = _context.Products.AsQueryable();

            // Lọc theo danh mục nếu có
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            // Áp dụng bộ lọc giá dùng chung của ông giáo
            query = ApplyPriceFilter(query, priceRange);

            var totalItems = await query.CountAsync();
            var products = await query
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Đẩy dữ liệu phân trang và bộ lọc hiện tại ra ViewBag để View bắt được
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentPriceRange = priceRange;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(products);
        }

        // Đường dẫn bắt buộc phải trùng khớp: /Product/Search
        [HttpGet]
        public async Task<IActionResult> Search(string q) // Tên tham số phải trùng với thuộc tính name="q" của ô input
        {
            // Giữ lại từ khóa hiển thị ra giao diện nếu cần
            ViewBag.SearchKeyword = q;

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                // Tìm kiếm gần đúng theo tên sản phẩm
                query = query.Where(p => p.ProductName.Contains(q));
            }

            var results = await query
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            // Trả kết quả về chung với giao diện trang Index để hiển thị danh sách lưới sản phẩm
            return View("Index", results);
        }

        [HttpGet]
        public async Task<IActionResult> SmartSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            // Tìm kiếm sản phẩm theo tên (chứa từ khóa), lấy tối đa 6 kết quả để không bị quá dài
            var results = await _context.Products
                .Where(p => p.ProductName.Contains(term))
                .Take(6)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.ProductName,
                    image = string.IsNullOrEmpty(p.MainImage) ? "/images/default-product.jpg" : p.MainImage,
                    // Tính toán giá hiển thị nếu sản phẩm đang được giảm giá
                    price = (p.IsDiscount == true && p.DiscountPrice.HasValue && p.DiscountPrice > 0)
                            ? p.DiscountPrice.Value
                            : (p.Price ?? 0)
                })
                .ToListAsync();

            return Json(results);
        }

        // 2. Trang Phụ Kiện (Đã thêm lọc giá và phân trang)
        [Route("phu-kien")]
        public async Task<IActionResult> Accessories(string priceRange, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;

            // Lọc sản phẩm không phải là thú cưng (IsPet == false)
            var query = _context.Products.Where(p => p.IsPet == false).AsQueryable();

            // Áp dụng bộ lọc giá dùng chung
            query = ApplyPriceFilter(query, priceRange);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Id) // Tránh dùng CreatedAt nếu DB không có
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CategoryName = "Phụ kiện thú cưng";
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(items);
        }

        // 3. Trang Chó Cảnh (Đã gộp 2 hàm cũ làm 1 + thêm lọc giá)
        [Route("cho-canh")]
        public async Task<IActionResult> ChoCanh(string priceRange, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;
            int categoryId = 1; // ID Chó cảnh trong DB

            var query = _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId && p.IsPet == true)
                .AsQueryable();

            // Áp dụng bộ lọc giá dùng chung
            query = ApplyPriceFilter(query, priceRange);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CategoryName = "Chó cảnh";
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(items);
        }

        // 4. Trang Mèo Cảnh (Đã tích hợp thêm lọc giá)
        [Route("meo-canh")]
        public async Task<IActionResult> MeoCanh(string priceRange, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;
            int categoryId = 2; // ID Mèo cảnh trong DB

            var query = _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsPet == true)
                .AsQueryable();

            // Áp dụng bộ lọc giá dùng chung
            query = ApplyPriceFilter(query, priceRange);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CategoryName = "Mèo cảnh";
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View("MeoCanh", items);
        }



        // 5. Trang Chi tiết sản phẩm (Giữ nguyên của ông)
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToListAsync();

            return View(product);
        }

        // 5. Trang Thức ăn thú cưng
        [Route("thuc-an")]
        public async Task<IActionResult> ThucAn(string priceRange, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;
            int categoryId = 4; // !!! Cần check lại Id của danh mục Thức ăn trong DB của ông giáo

            var query = _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsPet == false)
                .AsQueryable();

            query = ApplyPriceFilter(query, priceRange);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CategoryName = "Thức ăn thú cưng";
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Dùng chung file hiển thị với trang Phụ kiện
            return View("Accessories", items);
        }

        // 6. Trang Vệ sinh / Chăm sóc
        [Route("ve-sinh-cham-soc")]
        public async Task<IActionResult> VeSinhChamSoc(string priceRange, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;
            int categoryId = 5; // !!! Cần check lại Id của danh mục Vệ sinh/Chăm sóc trong DB của ông giáo

            var query = _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsPet == false)
                .AsQueryable();

            query = ApplyPriceFilter(query, priceRange);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CategoryName = "Vệ sinh & Chăm sóc";
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Tiếp tục dùng chung file với trang Phụ kiện
            return View("Accessories", items);
        }

        // 7. Trang Danh sách Dịch vụ Spa / Chăm sóc
        [Route("dich-vu")]
        public IActionResult Services()
        {
            ViewBag.CategoryName = "Dịch vụ Spa & Hotel";
            return View();
        }

        // --- HÀM TRỢ GIÚP (Helper) ĐỂ LỌC GIÁ DÙNG CHUNG ---
        // Viết riêng ra đây để không phải copy paste đoạn switch-case 3 lần
        private IQueryable<Product> ApplyPriceFilter(IQueryable<Product> query, string priceRange)
        {
            if (!string.IsNullOrEmpty(priceRange))
            {
                query = priceRange switch
                {
                    "under-500" => query.Where(p => (p.IsDiscount == true ? p.DiscountPrice : p.Price) < 500000),
                    "500-2000" => query.Where(p => (p.IsDiscount == true ? p.DiscountPrice : p.Price) >= 500000 && (p.IsDiscount == true ? p.DiscountPrice : p.Price) <= 2000000),
                    "2000-5000" => query.Where(p => (p.IsDiscount == true ? p.DiscountPrice : p.Price) >= 2000000 && (p.IsDiscount == true ? p.DiscountPrice : p.Price) <= 5000000),
                    "over-5000" => query.Where(p => (p.IsDiscount == true ? p.DiscountPrice : p.Price) > 5000000),
                    _ => query
                };
            }
            return query;
        }
    }
}