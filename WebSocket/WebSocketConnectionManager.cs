using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using WebSockets = System.Net.WebSockets;


namespace Diplom.WebSocket
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSockets.WebSocket> _connections = new ConcurrentDictionary<string, WebSockets.WebSocket>();

        public WebSockets.WebSocket GetConnection(string userId)
        {
            return _connections.TryGetValue(userId, out WebSockets.WebSocket connection) ? connection : null;
        }

        public void AddConnection(string userId, WebSockets.WebSocket webSocket)
        {
            _connections.TryAdd(userId, webSocket);
        }

        public void RemoveConnection(string userId)
        {
            _connections.TryRemove(userId, out _);
        }


    }
}
