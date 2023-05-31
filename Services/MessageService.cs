using Diplom.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using WebSockets = System.Net.WebSockets;
using System.Threading.Tasks;
using Diplom.Entities;

namespace Diplom.Services
{
    public interface IMessageService
    {
        Task SaveMessage(string senderId, string receiverId, string content);
    }


    public class MessageService : IMessageService
    {
        //private readonly WebSocketConnectionManager _connectionManager;

        //public MessageService(WebSocketConnectionManager connectionManager)
        //{
        //    _connectionManager = connectionManager;
        //}

        //public async Task SendMessageToUser(string userId, string message)
        //{
        //    WebSockets.WebSocket connection = _connectionManager.GetConnection(userId);

        //    if (connection != null && connection.State == WebSocketState.Open)
        //    {
        //        // Відправка повідомлення через WebSocket підключення
        //        // Наприклад:
        //        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        //        await connection.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        //    }
        //}
        private readonly ApplicationContext _dbContext;

        public MessageService(ApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveMessage(string senderId, string receiverId, string content)
        {
            var message = new Message
            {
                SenderId = senderId,
                RecipientEmail = receiverId,
                Text = content,
                Date = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
        }
    }
}
