using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ManaPet _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IPhotoService _photoService;

        public ProductController(ManaPet context, IWebHostEnvironment hostEnvironment, IPhotoService photoService)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _photoService = photoService;
        }

        // 1. TRANG DANH SÁCH
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return View(products);
        }

        // 2. TRANG THÊM MỚI (GET)

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName");
            return View(); // Trả về View trống thôi
        }

        // 3. XỬ LÝ LƯU SẢN PHẨM (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? mainImageFile, List<IFormFile> moreImages)
        {
            // 1. Kiểm tra ảnh chính
            if (mainImageFile == null || mainImageFile.Length == 0)
            {
                ModelState.AddModelError("MainImage", "Vui lòng chọn ảnh chính cho sản phẩm!");
            }

            // 2. Kiểm tra trùng tên (Bọc kỹ để không sập)
            var isExist = await _context.Products.AnyAsync(p => p.ProductName.ToLower().Trim() == product.ProductName.ToLower().Trim());
            if (isExist)
            {   
                ModelState.AddModelError("ProductName", "Tên sản phẩm này đã tồn tại rồi!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // A. XỬ LÝ ẢNH CHÍNH
                    if (mainImageFile != null)
                    {
                        var result = await _photoService.AddPhotoAsync(mainImageFile);
                        if (result.Error == null)
                        {
                            product.MainImage = result.SecureUrl.AbsoluteUri; // Link Cloudinary
                        }
                    }

                    // B. TÍNH TOÁN GIÁ GIẢM (Sửa chỗ này để tránh lỗi Null decimal?)
                    if (product.IsDiscount == true && product.DiscountPercent > 0 && product.Price.HasValue)
                    {
                        // Dùng .Value để chắc chắn có số mới tính
                        decimal discountAmount = product.Price.Value * (decimal)product.DiscountPercent / 100;
                        product.DiscountPrice = product.Price.Value - discountAmount;
                    }
                    else
                    {
                        product.DiscountPrice = null;
                    }

                    //product.CreatedAt = DateTime.Now;

                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    // C. XỬ LÝ ALBUM ẢNH
                    if (moreImages != null && moreImages.Count > 0)
                    {
                        foreach (var file in moreImages)
                        {
                            if (file.Length > 0)
                            {
                                var result = await _photoService.AddPhotoAsync(file);
                                if (result.Error == null)
                                {
                                    _context.ProductImages.Add(new ProductImage
                                    {
                                        ProductId = product.Id,
                                        ImageUrl = result.SecureUrl.AbsoluteUri, // Link Cloudinary
                                        IsDefault = false
                                    });
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // "Lưới bảo hiểm" cuối cùng: Không cho sập app, báo lỗi ra màn hình
                    ModelState.AddModelError("", "Lỗi hệ thống khi lưu: " + ex.Message);
                }
            }

            // Nếu có lỗi (trùng tên, thiếu ảnh, hoặc catch lỗi DB), quay lại View
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // Hàm SaveFile giữ nguyên vì ông viết chuẩn rồi
        private async Task<string> SaveFile(IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(wwwRootPath, @"images/products/");

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return "/images/products/" + fileName;
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? mainImageFile, List<IFormFile> moreImages)
        {
            if (id != product.Id) return NotFound();

            // 1. Check trùng tên (loại trừ chính nó)
            if (await _context.Products.AnyAsync(p => p.ProductName == product.ProductName && p.Id != id))
            {
                ModelState.AddModelError("ProductName", "Tên này bị trùng với sản phẩm khác rồi ông giáo!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // A. Xử lý ảnh chính: Nếu có file mới thì lưu, không thì giữ nguyên ảnh cũ trong Hidden Input
                    if (mainImageFile != null && mainImageFile.Length > 0)
                    {
                        var result = await _photoService.AddPhotoAsync(mainImageFile);
                        if (result.Error == null)
                        {
                            product.MainImage = result.SecureUrl.AbsoluteUri;
                        }
                    }

                    // B. Tính toán lại giá giảm
                    if (product.IsDiscount == true && product.DiscountPercent > 0 && product.Price.HasValue)
                    {
                        decimal discountAmount = product.Price.Value * (decimal)product.DiscountPercent / 100;
                        product.DiscountPrice = product.Price.Value - discountAmount;
                    }
                    else
                    {
                        product.DiscountPrice = null;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // C. Thêm ảnh vào Album (nếu có chọn thêm)
                    if (moreImages != null && moreImages.Count > 0)
                    {
                        foreach (var file in moreImages)
                        {
                            var result = await _photoService.AddPhotoAsync(file);
                            if (result.Error == null)
                            {
                                _context.ProductImages.Add(new ProductImage
                                {
                                    ProductId = product.Id,
                                    ImageUrl = result.SecureUrl.AbsoluteUri
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "CategoryName", product.CategoryId);
            return View(product);
        }
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // DELETE (Sử dụng AJAX cho xịn)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return Json(new { success = false });

            // Xóa file vật lý trước khi xóa bản ghi
            if (!string.IsNullOrEmpty(product.MainImage))
            {
                var oldPath = Path.Combine(_hostEnvironment.WebRootPath, product.MainImage.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Exists(oldPath);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteImage(int id)
        {
            var img = await _context.ProductImages.FindAsync(id);
            if (img == null) return Json(new { success = false, message = "Không tìm thấy ảnh!" });

            try
            {
                // 1. Xóa file vật lý trong thư mục wwwroot
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // 2. Xóa trong Database
                _context.ProductImages.Remove(img);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

    }


}