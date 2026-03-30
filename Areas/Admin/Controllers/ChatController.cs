using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChatController : Controller
    {
        private readonly ManaPet _db;

        public ChatController(ManaPet db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách hội thoại, kèm theo thông tin User và tin nhắn cuối cùng
            var conversations = await _db.Conversations
                .Include(c => c.User)
                .ThenInclude(u => u.UserProfile)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(conversations);
        }

        
        // Hàm lấy nội dung tin nhắn của một cuộc hội thoại cụ thể
        [HttpGet]
        public async Task<IActionResult> GetMessages(int id)
        {
            var messages = await _db.Messages
                .Where(m => m.ConversationId == id)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
            return PartialView("_MessageList", messages);
        }

        [HttpPost]
        public async Task<IActionResult> SaveMessage(int conversationId, string text)
        {
            if (string.IsNullOrEmpty(text)) return BadRequest();

            var msg = new Message
            {
                ConversationId = conversationId,
                MessageText = text,
                SenderType = "Admin", // Vì đây là Controller của Admin
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();

            return Ok(); // Trả về 200 OK cho Ajax
        }
    }
}
