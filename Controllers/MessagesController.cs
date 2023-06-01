using Diplom.Auth;
using Diplom.Entities;
using Diplom.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Diplom.WebSocket;
using Diplom.Services;
using WebSockets = System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace Diplom.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MessagesController : ControllerBase
    {

        //private readonly ApplicationContext _context;

        //public MessagesController(ApplicationContext context)
        //{
        //    _context = context;
        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateMessage(CreateMessageModel model)
        //{
        //    var user = (User)HttpContext.Items["User"];
        //    Message message = new Message
        //    {
        //        SenderId = user.Id,
        //        RecipientEmail = model.RecipientEmail,
        //        Text = model.Text,
        //        Date = model.Date,

        //    };
        //    // Перевірити, чи вказаний користувач є отримувачем повідомлення
        //    if (message.RecipientEmail == user.Email)
        //    {
        //        return BadRequest("Неприпустимий отримувач повідомлення");
        //    }

        //    _context.Messages.Add(message);
        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetMessages()
        //{

        //    var user = (User)HttpContext.Items["User"];

        //    //Отримати список повідомлень для користувача за його ID
        //    var messages = _context.Messages
        //        .Where(m => m.RecipientEmail == user.Email)
        //        .ToList();
        //    //var email = GetAllMessages(user.Email);

        //    return Ok(messages);
        //}

        //public List<Message> GetAllMessages(string email)
        //{
        //    return _context.Messages
        //        .Where(m => m.RecipientEmail == email).ToList();
        //    //.ToListAsync();

        //}
        //-------------------------------------------------------------------------------------------
        //private readonly ILogger<MessagesController> _logger;
        //private readonly WebSocketConnectionManager _connectionManager;
        //private readonly IMessageService _messageService;
        //private readonly ApplicationContext _context;

        //public MessagesController(ILogger<MessagesController> logger, 
        //    WebSocketConnectionManager connectionManager, 
        //    IMessageService messageService,
        //    ApplicationContext context)
        //{
        //    _logger = logger;
        //    _connectionManager = connectionManager;
        //    _messageService = messageService;
        //    _context = context;
        //}


        //[HttpGet]
        //public async Task<IActionResult> GetMessages()
        //{

        //    var user = (User)HttpContext.Items["User"];

        //    //Отримати список повідомлень для користувача за його ID
        //    var messages = _context.Messages
        //        .Where(m => m.RecipientEmail == user.Email)
        //        .ToList();
        //    //var email = GetAllMessages(user.Email);

        //    return Ok(messages);
        //}


        //[HttpPost()]
        //public async Task<IActionResult> SendMessage(/*string senderId, string receiverId, [FromBody] string content*/ CreateMessageModel model)
        //{
        //    try
        //    {
        //        var user = (User)HttpContext.Items["User"];
        //        Message message = new Message
        //        {
        //            SenderId = user.Id,
        //            RecipientEmail = model.RecipientEmail,
        //            Text = model.Text,
        //            Date = model.Date,

        //        };

        //        // Відправити повідомлення до отримувача
        //        await SendMessageToUser(message.SenderId, message.RecipientEmail, message.Text);

        //        // Зберегти повідомлення в базі даних
        //        await _messageService.SaveMessage(message.SenderId, message.RecipientEmail, message.Text);

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error sending message.");
        //        return StatusCode(500, "Error sending message.");
        //    }
        //}

        //private async Task SendMessageToUser(string senderId, string RecipientEmail, string text)
        //{
        //    WebSockets.WebSocket connection = _connectionManager.GetConnection(RecipientEmail);

        //    if (connection != null && connection.State == WebSockets.WebSocketState.Open)
        //    {
        //        // Відправка повідомлення через WebSocket підключення
        //        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(text));
        //        await connection.SendAsync(buffer, WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
        //    }
        //}

        //[HttpGet("ws")]
        //public async Task<IActionResult> ConnectWebSocket()
        //{

        //    if (!HttpContext.WebSockets.IsWebSocketRequest)
        //    {
        //        return BadRequest();
        //    }

        //    if (!HttpContext.WebSockets.IsWebSocketRequest)
        //    {
        //        return BadRequest();
        //    }

        //    var user = (User)HttpContext.Items["User"];
        //    var userTest = "438426b2-3772-4c9a-bbc7-0310a0093042";
        //    WebSockets.WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        //    _connectionManager.AddConnection(/*user.Id*/userTest, webSocket);

        //    await ReceiveMessages(/*user.Id*/userTest, webSocket);

        //    return new EmptyResult();
        //}

        //private async Task ReceiveMessages(string userId, WebSockets.WebSocket webSocket)
        //{
        //    var buffer = new byte[1024 * 4];
        //    WebSockets.WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        //    while (!result.CloseStatus.HasValue)
        //    {
        //        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

        //        // Зберегти отримане повідомлення в базі даних
        //        await _messageService.SaveMessage(userId, userId, message);

        //        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //    }

        //    _connectionManager.RemoveConnection(userId);
        //    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        //}

        //[HttpGet("/ws")]
        //public async Task Get()
        //{
        //    if (HttpContext.WebSockets.IsWebSocketRequest)
        //    {
        //        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        //        _logger.Log(LogLevel.Information, "WebSocket connection established");
        //        await Echo(webSocket);
        //    }
        //    else
        //    {
        //        HttpContext.Response.StatusCode = 400;

        //    }
        //}

        //private async Task Echo(WebSockets.WebSocket webSocket)
        //{
        //    var buffer = new byte[1024 * 4];
        //    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //    _logger.Log(LogLevel.Information, "Message received from Client");

        //    while (!result.CloseStatus.HasValue)
        //    {
        //        var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
        //        await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        //        _logger.Log(LogLevel.Information, "Message sent to Client");

        //        buffer = new byte[1024 * 4];
        //        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //        _logger.Log(LogLevel.Information, "Message received from Client");

        //    }
        //    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        //    _logger.Log(LogLevel.Information, "WebSocket connection closed");
        //}
    }
}
