using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TopServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();

            //task1.Wait();

            Console.WriteLine("Press return to exit the server.");
            Console.ReadLine();
        }
        public static void Run()
        {
            HttpListener s = new HttpListener();
            s.Prefixes.Add("http://localhost:8000/ws/");
            s.Start();

            var listen_task = s.GetContextAsync();

            listen_task.ContinueWith(async (hc_task) => 
            {
                var hc = hc_task.Result;

                if (!hc.Request.IsWebSocketRequest)
                {
                    hc.Response.StatusCode = 400;
                    hc.Response.Close();
                    return;
                }

                var wsc = await hc.AcceptWebSocketAsync(null);

                var ws = wsc.WebSocket;

                for (int i = 0; i != 10; ++i)
                {
                    await Task.Delay(1000);
                    var time = DateTime.Now.ToLongTimeString();
                    var buffer = Encoding.UTF8.GetBytes(time);
                    var segment = new ArraySegment<byte>(buffer);
                    await ws.SendAsync(segment, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                }
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            });
        }
    }
}
