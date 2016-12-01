using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TopClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientTask1 = Client();
            Console.ReadLine();
        }

        static async Task Client()
        {
            ClientWebSocket ws = new ClientWebSocket();
            Console.WriteLine("Press return to exit the client.");
            var uri = new Uri("ws://localhost:8000/ws/");

            await ws.ConnectAsync(uri, CancellationToken.None);
            var buffer = new byte[1024];
            while (true)
            {
                var segment = new ArraySegment<byte>(buffer);

                var result = await ws.ReceiveAsync(segment, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "I don't do binary", CancellationToken.None);
                    return;
                }

                int count = result.Count;
                while (!result.EndOfMessage)
                {
                    if (count >= buffer.Length)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "That's too long", CancellationToken.None);
                        return;
                    }

                    segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                    result = await ws.ReceiveAsync(segment, CancellationToken.None);
                    count += result.Count;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, count);
                Console.WriteLine(">" + message);
            }

        }
    }
}
