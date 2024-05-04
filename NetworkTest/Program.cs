namespace NetworkTest;

using System.Text;
using System.Net;
using System.Net.Sockets;

public static class NetworkTest {

    private const int Port = 80;
    private const string ServerIP = "192.168.2.41";

    static async Task Main() {
        Console.WriteLine("Enter C for Client S for Server");
        string mode = Console.ReadLine() ?? throw new ArgumentNullException();

        if (mode.Equals("C", StringComparison.InvariantCultureIgnoreCase)) {
            await CreateClient();
            return;
        }
        if (mode.Equals("S", StringComparison.InvariantCultureIgnoreCase)) {
            await CreateHost();
        return;
        }
        throw new ArgumentException();
    }
    
    static async Task CreateClient() {
        IPAddress ipAddress = IPAddress.Parse(ServerIP);
        var ipEndPoint = new IPEndPoint(ipAddress, Port);

        using TcpClient client = new();
        await client.ConnectAsync(ipEndPoint);
        Console.WriteLine("Created client");

        await using NetworkStream stream = client.GetStream();

        ReceiveMessages(stream);
        await SendMessages(stream);
    }

    static async Task CreateHost() {
        var ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
        TcpListener listener = new(ipEndPoint);
        try {
            listener.Start();
            Console.WriteLine("Created host");

            using TcpClient handler = await listener.AcceptTcpClientAsync();
            await using NetworkStream stream = handler.GetStream();

            ReceiveMessage(stream);
            await SendMessages(stream);
        }
        finally {
            listener.Stop();
        }
    }

    static async Task SendMessage(NetworkStream stream, string message) {
        var bytes = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(bytes);
    }

    static async Task ReceiveMessage(NetworkStream stream) {
        var buffer = new byte[1_024];
        int received = await stream.ReadAsync(buffer);
        string message = Encoding.UTF8.GetString(buffer, 0, received);
        Console.WriteLine($"Other server: {message}");
    }

    async static Task ReceiveMessages(NetworkStream stream) {
        while (true) {
            await ReceiveMessage(stream);
        }
    }

    async static Task SendMessages(NetworkStream stream) {
        while (true) {
            string? message = Console.ReadLine();
            if (message is not null) {
                await SendMessage(stream, message);
            }
        }
    }
}