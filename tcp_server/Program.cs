using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

IPAddress address = IPAddress.Loopback;
IPEndPoint serverEndpoint = new(address, 8000);

using Socket listener = new(serverEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
listener.Bind(serverEndpoint);
listener.Listen(10);

Console.WriteLine("Server started. Waiting for connection...");

using Socket handler = await listener.AcceptAsync();
Console.WriteLine("Client connected.");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Shutdown requested...");
    cts.Cancel();
    e.Cancel = true;
};

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        var buffer = new byte[1024];
        var received = await handler.ReceiveAsync(buffer, SocketFlags.None, cts.Token);

        if (received == 0)
        {
            Console.WriteLine("Client disconnected.");
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, received);
        Console.WriteLine($"Server received: {message}");

        var response = Encoding.UTF8.GetBytes("Hello Client\n");
        await handler.SendAsync(response, SocketFlags.None, cts.Token);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Server loop cancelled.");
}
finally
{
    handler.Shutdown(SocketShutdown.Both);
    handler.Close();
    listener.Close();
    Console.WriteLine("Server shut down gracefully.");
}