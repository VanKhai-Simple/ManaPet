using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petshop_frontend.Models;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly ManaPet _context;
        public UserProfileController(ManaPet context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserProfile model)
        {
            _context.UserProfiles.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
