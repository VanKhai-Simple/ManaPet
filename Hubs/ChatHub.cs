using Microsoft.AspNetCore.SignalR;
using Petshop_frontend.Models;
using Microsoft.EntityFrameworkCore;

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
                // Chuyển userId từ string sang int? một cách an toàn
                int? userId = null;
                if (!string.IsNullOrEmpty(userIdStr) && userIdStr != "null")
                {
                    if (int.TryParse(userIdStr, out int parsedId)) userId = parsedId;
                }

                // 1. Tìm cuộc hội thoại
                var conversation = _db.Conversations
                    .FirstOrDefault(c => (c.IsClosed == false) && // EF Core dịch được cái này
                                         ((userId != null && c.UserId == userId) ||
                                          (userId == null && c.GuestSessionId == senderSessionId)));

                // 2. Nếu chưa có thì tạo mới
                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        UserId = userId,
                        GuestSessionId = userId == null ? senderSessionId : null,
                        CreatedAt = DateTime.Now,
                        IsClosed = false
                    };
                    _db.Conversations.Add(conversation);
                    _db.SaveChanges(); // LƯU TẠI ĐÂY ĐỂ CÓ ID
                }

                // 3. Tạo tin nhắn mới
                var newMessage = new Message
                {
                    ConversationId = conversation.Id,
                    SenderType = userRole, // "Admin", "Guest", hoặc "Customer"
                    SenderId = userId,
                    MessageText = messageText,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                    
                _db.Messages.Add(newMessage);
                int rows = _db.SaveChanges(); // Kiểm tra xem có lưu được dòng nào không

                if (rows > 0)
                {
                    // Chỉ khi lưu DB thành công mới bắn SignalR (để đảm bảo tính đồng bộ)
                    await Clients.All.SendAsync("ReceiveMessage", userRole, messageText, DateTime.Now.ToString("HH:mm"), senderSessionId);
                }
            }
            catch (Exception ex)
            {
                // XEM LỖI Ở ĐÂY: Nhấn Ctrl + Alt + O trong Visual Studio để xem cửa sổ Output
                System.Diagnostics.Debug.WriteLine("LỖI LƯU DB: " + ex.ToString());
            }
        }
        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }
    }
}