using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Petshop_frontend.Models
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly ManaPet _db;
        public UserProfileController(ManaPet db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var profile = await _db.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                // Nếu chưa có profile (lỗi logic lúc tạo user), tạo mới luôn
                profile = new UserProfile { UserId = userId, FullName = User.Identity.Name };
                _db.UserProfiles.Add(profile);
                await _db.SaveChangesAsync();
            }
            return View(profile);
        }
    }
}
