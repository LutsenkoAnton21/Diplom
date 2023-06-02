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
using System.Security.Claims;

namespace Diplom.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/socket")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class SocketController : ControllerBase
    {
        const int MaxMessageSymbolsCount = 1024;
        const int StringTypeBytesCount = 4;

        private readonly int MessageBufferSize = MaxMessageSymbolsCount * StringTypeBytesCount;

        private readonly ILogger<SocketController> _logger;
        private readonly ApplicationContext _context;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IMessageService _messageService;

        WebSocketManager ContextWebSockets => HttpContext.WebSockets;
        bool IsNotWebSocketRequest => !ContextWebSockets.IsWebSocketRequest;
        User ConnectedUser => (User)HttpContext.Items["User"];

        public SocketController(
            ILogger<SocketController> logger,
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

        [HttpGet("message/all")]
        public IActionResult GetAllMessages()
        {
            _logger.Log(
                LogLevel.Information,
                $"Getting messages messages for {ConnectedUser.Email}..."
            );

            var messages = _context.Messages
                .Where(m => m.RecipientEmail == ConnectedUser.Email)
                .ToList();

            return Ok(messages);
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage(CreateMessageModel model)
        {
            try
            {
                var messageModel = new Message
                {
                    SenderId = ConnectedUser.Id,
                    RecipientEmail = model.RecipientEmail,
                    Text = model.Text,
                    Date = model.Date
                };

                _logger.Log(
                    LogLevel.Information,
                    $"Sending message to {messageModel.SenderId}..."
                );

                await SendMessage(
                    messageModel.SenderId,
                    messageModel.RecipientEmail,
                    messageModel.Text
                );

                await _messageService.SaveMessage(
                    messageModel.SenderId,
                    messageModel.RecipientEmail,
                    messageModel.Text
                );

                return new StatusCodeResult((int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error sending message to {model.RecipientEmail} from {ConnectedUser.Id}";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, errorMessage);
            }
        }

        [Route("")]
        public async Task<IActionResult> StartWebSocket()
        {
            if (IsNotWebSocketRequest)
            {
                return BadRequest();
            }
            _logger.Log(
                LogLevel.Information,
                $"Receiving messages for {ConnectedUser.Email}..."
            );

            var webSocket = await CreateConnection();
            var messageReceiveResult = await ReceiveMessages(ConnectedUser.Email, webSocket);
            await CloseConnection(ConnectedUser.Email, webSocket, messageReceiveResult);
            
            // TODO: Check why connection is closed

            return new StatusCodeResult((int)HttpStatusCode.Created);
        }

        private async Task SendMessage(string senderId, string recipientEmail, string text)
        {
            var webSocket = _connectionManager.GetConnection(recipientEmail);

            if (webSocket?.State == WebSockets.WebSocketState.Open)
            {
                await SendMessage(text, webSocket);
            }
        }

        private async Task SendMessage(
            string text,
            WebSockets.WebSocket webSocket
        )
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)),
                WebSockets.WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        private async Task<WebSockets.WebSocket> CreateConnection()
        {
            var webSocket = await ContextWebSockets.AcceptWebSocketAsync();
            _connectionManager.AddConnection(ConnectedUser.Email, webSocket);
            _logger.Log(
                LogLevel.Information, 
                $"Connection for user {ConnectedUser.Email} is established"
            );

            return webSocket;
        }

        private async Task<WebSockets.WebSocketReceiveResult> ReceiveMessages(string userId, WebSockets.WebSocket webSocket)
        {
            _logger.Log(
                LogLevel.Information,
                $"Receiving messages for {ConnectedUser.Email}..."
            );

            var messageBuffer = new byte[MessageBufferSize];
            var messageReceivedResult = await ReceiveMessage(messageBuffer, webSocket);

            while (!messageReceivedResult.CloseStatus.HasValue)
            {
                var messageContent = GetMessageContent(messageBuffer, messageReceivedResult);
                await _messageService.SaveMessage(userId, userId, messageContent);

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

        private string GetMessageContent(byte[] buffer, WebSockets.WebSocketReceiveResult result)
        {
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
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
                $"Connection for {ConnectedUser.Email} is closed"
            );
        }
    }
}
