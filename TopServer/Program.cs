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
            Console.WriteLine("The server is running.");

            try
            {
                Run();
            }
            catch (Exception e)
            {
                Console.Write($"An error has occurred while listening for the client connections: {e.Message}");
            }

            Console.ReadLine();
        }
        public static void Run()
        {
            HttpListener s = new HttpListener();
            s.Prefixes.Add("http://localhost:8000/ws/");
            s.Start();

            while (true)
            {
                var listen_task = s.GetContextAsync();

                listen_task.ContinueWith(async (hc_task) =>
                {
                    var hc = hc_task.Result;

                    if (hc.Request.IsWebSocketRequest)
                    {
                        var wsc = await hc.AcceptWebSocketAsync(null);

                        Console.WriteLine("A client has connected.");

                        var ws = wsc.WebSocket;

                        try
                        {
                            while (ws.State != WebSocketState.CloseReceived && ws.State != WebSocketState.Closed && ws.State != WebSocketState.Aborted)
                            {
                                await Task.Delay(1000);
                                var time = DateTime.Now.ToLongTimeString();
                                var buffer = Encoding.UTF8.GetBytes(time);
                                var segment = new ArraySegment<byte>(buffer);
                                await ws.SendAsync(segment, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                            }

                            Console.Write("The connection has been closed by the client.");
                        }
                        catch (Exception e)
                        {
                            Console.Write($"An error has occurred while sending the notifications to the client: {e.Message}");

                            try
                            {
                                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    else
                    {
                        hc.Response.StatusCode = 400;
                        hc.Response.Close();
                    }
                });

                listen_task.Wait();
            }
        }
    }
}
