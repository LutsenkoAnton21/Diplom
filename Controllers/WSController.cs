using System;
using System.Threading;
using System.Text;
using System.Linq;
using System.Net;
using WebSockets = System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Diplom.Entities;
using Diplom.Services;
using Diplom.Auth;
using Diplom.WebSocket;
using Diplom.Models;
using Newtonsoft.Json;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("api/socket/ws")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class WSSController : ControllerBase
    {
        const int MaxMessageSymbolsCount = 1024;
        const int StringTypeBytesCount = 4;

        private readonly int MessageBufferSize = MaxMessageSymbolsCount * StringTypeBytesCount;

        private readonly ILogger<WSController> _logger;
        private readonly ApplicationContext _context;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IMessageService _messageService;

        WebSocketManager ContextWebSockets => HttpContext.WebSockets;
        bool IsNotWebSocketRequest => !ContextWebSockets.IsWebSocketRequest;
        User ConnectedUser => (User)HttpContext.Items["User"];

        public WSSController(
            ILogger<WSController> logger,
            ApplicationContext context,
            WebSocketConnectionManager connectionManager,
            IMessageService messageService
        )
        {
            _logger = logger;
            _context = context;
            _connectionManager = connectionManager;
            _messageService = messageService;
        }

        [Route("")]
        public async Task<IActionResult> StartWebSocket()
        {
            if (IsNotWebSocketRequest)
            {
                return BadRequest();
            }

            var webSocket = await CreateConnection(ConnectedUser.Email);
            var messageReceiveResult = await ReceiveMessages(ConnectedUser.Email, webSocket);
            await CloseConnection(ConnectedUser.Email, webSocket, messageReceiveResult);

            // TODO: Check why connection is closed

            return new StatusCodeResult((int)HttpStatusCode.Created);
        }

        private async Task CloseConnection(
            string userEmail,
            WebSockets.WebSocket webSocket,
            WebSockets.WebSocketReceiveResult result
        )
        {
            _connectionManager.RemoveConnection(userEmail);
            await webSocket.CloseAsync(
                result.CloseStatus.Value,
                result.CloseStatusDescription,
                CancellationToken.None
            );

            _logger.Log(
                LogLevel.Information,
                $"Connection for {userEmail} is closed"
            );
        }

        private async Task<WebSockets.WebSocket> CreateConnection(string userEmail)
        {
            var webSocket = await ContextWebSockets.AcceptWebSocketAsync();
            _connectionManager.AddConnection(userEmail, webSocket);

            _logger.Log(
                LogLevel.Information,
                $"Connection for user {userEmail} is established"
            );

            return webSocket;
        }

        private IncommingMessageModel GetMessageContent(
            byte[] buffer,
            WebSockets.WebSocketReceiveResult result
        )
        {
            var contentString = Encoding.UTF8.GetString(buffer, 0, result.Count);
            return JsonConvert.DeserializeObject<IncommingMessageModel>(contentString);
        }

        private async Task<WebSockets.WebSocketReceiveResult> ReceiveMessages(
            string receiverEmail,
            WebSockets.WebSocket webSocket
        )
        {
            _logger.Log(
                LogLevel.Information,
                $"Receiving and saving messages for {receiverEmail}..."
            );

            var messageBuffer = new byte[MessageBufferSize];
            var messageReceivedResult = await ReceiveMessage(messageBuffer, webSocket);

            while (!messageReceivedResult.CloseStatus.HasValue)
            {
                var messageContent = GetMessageContent(messageBuffer, messageReceivedResult);
                await _messageService.SaveMessage(
                    messageContent.SenderEmail,
                    receiverEmail,
                    messageContent.Message
                );

                messageReceivedResult = await ReceiveMessage(messageBuffer, webSocket);
            }

            return messageReceivedResult;
        }

        private async Task<WebSockets.WebSocketReceiveResult> ReceiveMessage(
            byte[] messageBuffer,
            WebSockets.WebSocket webSocket
        )
        {
            return await webSocket.ReceiveAsync(
                new ArraySegment<byte>(messageBuffer),
                CancellationToken.None
            );
        }
    }
}
