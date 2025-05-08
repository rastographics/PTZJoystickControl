using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace PtzJoystickControl.Application.Services
{
    public class WebSocketHandler
    {
        private readonly List<WebSocket> _sockets = new List<WebSocket>();

        public async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            _sockets.Add(webSocket);
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            _sockets.Remove(webSocket);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async Task NotifyClientsAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            foreach (var socket in _sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
