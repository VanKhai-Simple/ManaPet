using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Petshop_frontend.Models;

namespace Petshop_frontend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatHub(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        // Bỏ hết đống logic check Conversation rườm rà. Chỉ cần ID!
        public async Task SendMessage(int conversationId, int userId, string messageText, string userRole)
        {
            string timeStr = DateTime.Now.ToString("HH:mm");

            // 1. FIRE: Bắn tin đi ngay lập tức cho tất cả (Client & Admin sẽ tự lọc theo conversationId)
            await Clients.All.SendAsync("ReceiveMessage", userRole, messageText, timeStr, conversationId, userId);

            // 2. FORGET: Đẩy việc lưu DB vào Background Thread (Không làm nghẽn SignalR)
            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ManaPet>();
                try
                {
                    db.Messages.Add(new Message
                    {
                        ConversationId = conversationId,
                        SenderType = userRole,
                        SenderId = (userRole == "Admin") ? null : userId,
                        MessageText = messageText,
                        IsRead = (userRole == "Admin"),
                        CreatedAt = DateTime.Now
                    });
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Hub DB Error]: {ex.Message}");
                }
            });
        }
    }
}