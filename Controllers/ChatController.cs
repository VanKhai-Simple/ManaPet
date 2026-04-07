using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Controllers
{
    // Đảm bảo Route như thế này
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ManaPet _db;
        public ChatController(ManaPet db) => _db = db;


        // Route này sẽ khớp với: /api/chat/GetHistory/{userId}
        [HttpGet("GetHistory/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            // 1. CHẶN LỖI: Nếu userId = 0 (chưa login hoặc lỗi js) thì trả về lỗi nhẹ nhàng, đừng để nổ 500
            if (userId <= 0) return BadRequest(new { success = false, message = "User không hợp lệ" });

            try
            {
                var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.UserId == userId && c.IsClosed == false);

                if (conv == null)
                {
                    conv = new Conversation { UserId = userId, CreatedAt = DateTime.Now, IsClosed = false };
                    _db.Conversations.Add(conv);
                    await _db.SaveChangesAsync();
                }

                var messages = await _db.Messages
                    .Where(m => m.ConversationId == conv.Id)
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new {
                        sender = m.SenderType,
                        text = m.MessageText,
                        time = (m.CreatedAt ?? DateTime.Now).ToString("HH:mm")
                    }).ToListAsync();

                return Ok(new { success = true, conversationId = conv.Id, data = messages });
            }
            catch (Exception ex)
            {
                // Log lỗi ra để debug nếu cần
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}