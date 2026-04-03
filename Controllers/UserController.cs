using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;
using System.Security.Claims;
using BC = BCrypt.Net.BCrypt;


namespace Petshop_frontend.Controllers
{
    public class UserController : Controller
    {
        private readonly ManaPet _db;

        public UserController(ManaPet db)
        {
            _db = db;
        }

        // GET: Register
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Kiểm tra trùng Username
            if (_db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã được sử dụng.");
                return View(model);
            }

            // 2. Tạo User và Hash mật khẩu
            var user = new User
            {
                Username = model.Username,
                PasswordHash = BC.HashPassword(model.Password),
                Role = "Customer",
                Status = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(); // Lưu để lấy UserId tự tăng

            // 3. Tự động tạo Profile liên kết với UserId vừa tạo
            var profile = new UserProfile
            {
                UserId = user.Id,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone
            };
            _db.UserProfiles.Add(profile);

            // 4. Tự động tạo Giỏ hàng cho User mới
            var cart = new Cart { UserId = user.Id };
            _db.Carts.Add(cart);

            await _db.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string sessionId, string userId)
        {
            int? uId = (string.IsNullOrEmpty(userId) || userId == "null") ? null : int.Parse(userId);

            var conversation = _db.Conversations
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.IsClosed == false && (
                    (uId != null && c.UserId == uId) ||
                    (uId == null && c.GuestSessionId == sessionId)
                ));

            if (conversation == null) return Json(new { success = false });

            var messages = conversation.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new {
                        sender = m.SenderType, // "Admin" hoặc "Customer"
                        text = m.MessageText,
                        time = m.CreatedAt.HasValue ? m.CreatedAt.Value.ToString("HH:mm") : ""
                    }).ToList();

            return Json(new { success = true, data = messages });
        }

        // GET: Login
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Tìm user trong DB theo Username
            var user = _db.Users.FirstOrDefault(u => u.Username == model.Username);

            // Kiểm tra mật khẩu bằng BCrypt
            if (user != null && BC.Verify(model.Password, user.PasswordHash))
            {
                // Thiết lập các thông tin định danh (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username!),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role ?? "Customer")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                // Đăng nhập vào hệ thống Cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (user.Role == "Admin")
                {
                    // Chuyển hướng vào Area Admin, Controller Home, Action Index
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác.");
            return View(model);
        }



        // 1. Action này để "đẩy" User sang trang đăng nhập của Google
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                // 1. Lấy kết quả từ Google (Phải dùng GoogleDefaults để đọc Claims)

                var result = await HttpContext.AuthenticateAsync("ExternalCookies");
                if (!result.Succeeded || result.Principal == null) return RedirectToAction("Login");

                // 2. Trích xuất thông tin
                var email = result.Principal.FindFirstValue(ClaimTypes.Email);
                var name = result.Principal.FindFirstValue(ClaimTypes.Name);
                var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

                // 3. Kiểm tra User dựa trên Email
                var profile = _db.UserProfiles.Include(p => p.User)
                                              .FirstOrDefault(p => p.Email == email);

                User? user;

                if (profile == null)
                {
                    user = new User
                    {
                        Username = email,
                        Provider = "Google",
                        GoogleId = googleId,
                        ExternalId = googleId,
                        Role = "Customer",
                        Status = true,
                        CreatedAt = DateTime.Now
                    };

                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();

                    var newProfile = new UserProfile
                    {
                        UserId = user.Id,
                        Email = email,
                        FullName = name,
                        Avatar = result.Principal.FindFirstValue("picture")
                    };
                    _db.UserProfiles.Add(newProfile);

                    // TỰ ĐỘNG TẠO GIỎ HÀNG (Bạn đã thêm ở Register thì Google Login cũng nên có)
                    var cart = new Cart { UserId = user.Id };
                    _db.Carts.Add(cart);

                    await _db.SaveChangesAsync();
                    profile = newProfile; // Gán lại để dùng cho Claims bên dưới
                }
                else
                {
                    user = profile.User;
                    if (user != null && string.IsNullOrEmpty(user.GoogleId))
                    {
                        user.GoogleId = googleId;
                        user.Provider = "Google";
                        _db.Users.Update(user);
                        await _db.SaveChangesAsync();
                    }
                }

                // 4. Đăng nhập vào hệ thống Cookie của ManaPet
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user?.Username ?? email!),
                    new Claim("UserId", user?.Id.ToString() ?? ""), // Đồng bộ với hàm Login thường của bạn
                    new Claim(ClaimTypes.Email, email!),
                    new Claim("FullName", profile?.FullName ?? name ?? ""),
                    new Claim(ClaimTypes.Role, user?.Role ?? "Customer")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Quan trọng: Sau khi xác thực Google xong, mình "đổi vé" sang Cookie để duy trì phiên đăng nhập
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Lấy lỗi chi tiết nhất từ SQL Server (InnerException)
                var innerError = ex.InnerException != null ? ex.InnerException.Message : "Không có lỗi sâu hơn";
                return Content($"Lỗi lưu DB: {ex.Message} | Chi tiết: {innerError}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FacebookLogin()
        {
            // Ép đăng xuất khỏi các scheme tạm để xóa data lỗi cũ nếu còn sót
            await HttpContext.SignOutAsync("ExternalCookies");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("FacebookResponse"),
                // Ép Facebook hiện bảng chọn tài khoản hoặc xác thực lại (nếu cần)
                Items = { { "prompt", "select_account" } }
            };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> FacebookResponse()
        {
            // 1. Lấy dữ liệu từ trạm trung chuyển ExternalCookies
            var result = await HttpContext.AuthenticateAsync("ExternalCookies");

            if (!result.Succeeded || result.Principal == null) return RedirectToAction("Login");

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var facebookId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Kiểm tra User dựa trên Email (hoặc FacebookId nếu muốn chắc chắn hơn)
            var profile = _db.UserProfiles.Include(p => p.User)
                                          .FirstOrDefault(p => p.Email == email && email != null);

            User? user;

            if (profile == null)
            {
                user = new User
                {
                    // Dự phòng nếu không có email thì dùng ID Facebook làm Username
                    Username = email ?? ("fb_" + facebookId),
                    Provider = "Facebook",
                    FacebookId = facebookId,
                    ExternalId = facebookId,
                    Role = "Customer",
                    Status = true,
                    CreatedAt = DateTime.Now
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                var newProfile = new UserProfile
                {
                    UserId = user.Id,
                    Email = email,
                    FullName = name,
                    // Link ảnh Facebook chuẩn xác hơn
                    Avatar = $"https://graph.facebook.com/{facebookId}/picture?type=large"
                };
                _db.UserProfiles.Add(newProfile);

                _db.Carts.Add(new Cart { UserId = user.Id });

                await _db.SaveChangesAsync();
                profile = newProfile;
            }
            else
            {
                user = profile.User;
                // Cập nhật FacebookId nếu trước đó User này chưa có (ví dụ đăng ký thường bằng Email)
                if (user != null && string.IsNullOrEmpty(user.FacebookId))
                {
                    user.FacebookId = facebookId;
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();
                }
            }

            // 3. Đăng nhập vào hệ thống Cookie chính thức
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user?.Username ?? name ?? "User"),
                new Claim("UserId", user?.Id.ToString() ?? ""),
                new Claim(ClaimTypes.Email, email ?? ""),
                new Claim("FullName", profile?.FullName ?? name ?? ""),
                new Claim(ClaimTypes.Role, user?.Role ?? "Customer")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // QUAN TRỌNG: Xóa cookie tạm External để sạch máy
            await HttpContext.SignOutAsync("ExternalCookies");

            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }
    }
}