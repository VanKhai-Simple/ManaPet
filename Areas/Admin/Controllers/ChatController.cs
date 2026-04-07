using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Areas.Admin.Controllers // Namespace phải có .Areas.Admin
{
    [Area("Admin")] // BẮT BUỘC có dòng này
    public class ChatController : Controller // Dùng Controller (không phải ControllerBase) để dùng được View()
    {
        private readonly ManaPet _db;
        public ChatController(ManaPet db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var conversations = await _db.Conversations
                .Include(c => c.User)
                    .ThenInclude(u => u.UserProfile) // Đi tiếp từ User sang Profile
                .Where(c => c.IsClosed == false)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(conversations);
        }

        [HttpGet]
        public async Task<IActionResult> GetChatHistory(int convId)
        {
            var messages = await _db.Messages
                .Where(m => m.ConversationId == convId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new {
                    m.SenderType,
                    m.MessageText,
                    CreatedAt = m.CreatedAt.HasValue ? m.CreatedAt.Value.ToString("HH:mm") : ""
                })
                .ToListAsync();

            return Json(messages);
        }
    }
}