// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CarePlatform.MockVistA;

/// <summary>
/// Minimal XWB Broker RPC server that handles only the RPCs required for
/// authentication and basic user info — enough to demo the example web frontend.
/// </summary>
public static class Program
{
    private const int Port = 9200;

    public static async Task Main(string[] args)
    {
        var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : Port;
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();

        Console.WriteLine($"Mock VistA RPC Server listening on tcp://localhost:{port}");
        Console.WriteLine("Credentials: AccessCode=cprs  VerifyCode=cprs1234");
        Console.WriteLine("Press Ctrl+C to stop.");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            while (!cts.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(cts.Token);
                _ = Task.Run(() => HandleClient(client, cts.Token));
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }

    private static async Task HandleClient(TcpClient client, CancellationToken ct)
    {
        var ep = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        Console.WriteLine($"[{ep}] Connected");

        try
        {
            using var stream = client.GetStream();
            var session = new ClientSession(stream, ep);
            await session.RunAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"[{ep}] Error: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine($"[{ep}] Disconnected");
        }
    }
}
