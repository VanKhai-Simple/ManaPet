using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Petshop_frontend.Models;

namespace Petshop_frontend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ManaPet _db;
        public ChatHub(ManaPet db) => _db = db;

        // Thêm tham số userRole để biết ai đang gửi (Admin/Guest/Customer)
        public async Task SendMessage(string senderSessionId, string messageText, string userIdStr, string userRole)
        {
            try
            {
                // 1. Phân tách ID
                int? userId = null;
                if (!string.IsNullOrEmpty(userIdStr) && userIdStr != "null" && userIdStr != "")
                {
                    if (int.TryParse(userIdStr, out int parsedId)) userId = parsedId;
                }

                // 2. KHAI BÁO BIẾN TRỐNG hoàn toàn (Không gán giá trị từ DB ở đây)
                Conversation conversation = null;

                // 3. LOGIC TÌM KIẾM PHÂN LUỒNG (Chỗ này sẽ quyết định null hay không)
                if (userId.HasValue)
                {
                    // Tìm theo UserId của người đã đăng nhập
                    conversation = _db.Conversations
                        .FirstOrDefault(c => c.IsClosed == false && c.UserId == userId);

                    // Nếu không thấy theo UserId, tìm xem có hội thoại vãng lai (Guest) để "nâng cấp" không
                    if (conversation == null)
                    {
                        conversation = _db.Conversations
                            .FirstOrDefault(c => c.IsClosed == false && c.GuestSessionId == senderSessionId);

                        if (conversation != null)
                        {
                            // NÂNG CẤP: Gán UserId vào hội thoại này
                            conversation.UserId = userId;
                            await _db.SaveChangesAsync();
                            Console.WriteLine($"[ChatHub] Đã gắn UserId {userId} vào Conversation {conversation.Id}");
                        }
                    }
                }
                else
                {
                    // Nếu là khách chưa đăng nhập, chỉ tìm theo SessionId
                    conversation = _db.Conversations
                        .FirstOrDefault(c => c.IsClosed == false && c.GuestSessionId == senderSessionId);
                }

                // 4. CHỖ NÀY MỚI LÀ CHỖ NULL NÀY
                // Nếu qua tất cả các bước tìm kiếm trên mà vẫn không thấy (Khách mới tinh)
                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        UserId = userId,
                        GuestSessionId = senderSessionId,
                        CreatedAt = DateTime.Now,
                        IsClosed = false
                    };
                    _db.Conversations.Add(conversation);
                    await _db.SaveChangesAsync();
                    Console.WriteLine($"[ChatHub] Tạo mới hội thoại Id: {conversation.Id}");
                }

                // 5. Tạo tin nhắn
                var newMessage = new Message
                {
                    ConversationId = conversation.Id,
                    SenderType = userRole,
                    SenderId = userId,
                    MessageText = messageText,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _db.Messages.Add(newMessage);

                if (await _db.SaveChangesAsync() > 0)
                {
                    string broadcastId = userId.HasValue ? userId.Value.ToString() : senderSessionId;
                    await Clients.All.SendAsync("ReceiveMessage", userRole, messageText, DateTime.Now.ToString("HH:mm"), broadcastId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI CHAT: " + ex.Message);
                throw;
            }
        }
        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }
    }
}