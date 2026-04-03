using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

            var filteredList = conversations
                .GroupBy(c => c.UserId ?? (object)c.GuestSessionId)
                .Select(g => g.First()) // Lấy bản ghi mới nhất của người đó
                .ToList();

            var unreadCounts = await _db.Messages
                .Where(m => m.SenderType != "Admin" && m.IsRead == false)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

            ViewBag.UnreadCounts = unreadCounts;

            return View(filteredList);
        }

        
        // Hàm lấy nội dung tin nhắn của một cuộc hội thoại cụ thể
        [HttpGet]
        public async Task<IActionResult> GetMessages(int id)
        {
            try
            {
                var unreadMessages = await _db.Messages
                .Where(m => m.ConversationId == id && m.SenderType != "Admin" && m.IsRead == false)
                .ToListAsync();

                    if (unreadMessages.Any())
                    {
                        unreadMessages.ForEach(m => m.IsRead = true);
                        await _db.SaveChangesAsync();
                    }

                var messages = await _db.Messages
                    .Where(m => m.ConversationId == id)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();
                return PartialView("_MessageList", messages);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy tin nhắn: " + ex.Message);
                return StatusCode(500, "Lỗi máy chủ");
            }

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
