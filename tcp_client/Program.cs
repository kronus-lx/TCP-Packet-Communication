using System.Net;
using System.Net.Sockets;
using System.Text;

IPAddress address = IPAddress.Loopback;
IPEndPoint ipEndPoint = new IPEndPoint(address, 8000);

using Socket client = new(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp
);

try
{
    await client.ConnectAsync(ipEndPoint);
    var message = "Hi Server";
    var messageBytes = Encoding.UTF8.GetBytes(message);
    _ = await client.SendAsync(messageBytes, SocketFlags.None);
    Console.WriteLine($"Socket client sent message: \"{message}\"");

    // Receive ack.
    var buffer = new byte[1_024];
    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
    var response = Encoding.UTF8.GetString(buffer, 0, received);
    Console.WriteLine($"Received Response from server- {response}");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    client.Shutdown(SocketShutdown.Both);
}
