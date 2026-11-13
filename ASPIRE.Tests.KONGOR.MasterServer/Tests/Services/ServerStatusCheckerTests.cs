using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Services;

public sealed class ServerStatusCheckerTests
{
    [Test]
    public async Task Invalid_Parameters_Return_False()
    {
        using ServerStatusChecker checker = new (NullLogger<ServerStatusChecker>.Instance);

        using (Assert.Multiple())
        {
            await Assert.That(await checker.CheckServerStatus(string.Empty, 11235, TimeSpan.FromMilliseconds(50))).IsFalse();
            await Assert.That(await checker.CheckServerStatus("   ", 11235, TimeSpan.FromMilliseconds(50))).IsFalse();
            await Assert.That(await checker.CheckServerStatus("127.0.0.1", 0, TimeSpan.FromMilliseconds(50))).IsFalse();
            await Assert.That(await checker.CheckServerStatus("127.0.0.1", -1, TimeSpan.FromMilliseconds(50))).IsFalse();
            await Assert.That(await checker.CheckServerStatus("127.0.0.1", 70000, TimeSpan.FromMilliseconds(50))).IsFalse();
        }
    }

    [Test]
    public async Task Uncached_Status_Returns_False()
    {
        using ServerStatusChecker checker = new (NullLogger<ServerStatusChecker>.Instance);

        await Assert.That(checker.GetCachedStatus("127.0.0.1", 11235)).IsFalse();
    }

    [Test]
    public async Task Responding_Udp_Server_Returns_True_And_Caches_Status()
    {
        using ServerStatusChecker checker = new (NullLogger<ServerStatusChecker>.Instance);

        // Bind an ephemeral UDP listener to simulate a game server
        using UdpClient mockGameServer = new (new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)mockGameServer.Client.LocalEndPoint!).Port;

        using CancellationTokenSource cts = new (TimeSpan.FromSeconds(3));

        // Background task to respond to the ping probe
        Task echoTask = Task.Run(async () =>
        {
            try
            {
                UdpReceiveResult request = await mockGameServer.ReceiveAsync(cts.Token);
                byte[] response = [0x01];
                await mockGameServer.SendAsync(response, response.Length, request.RemoteEndPoint);
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
        }, cts.Token);

        bool isAlive = await checker.CheckServerStatus("127.0.0.1", port, TimeSpan.FromSeconds(2));
        await echoTask;

        using (Assert.Multiple())
        {
            await Assert.That(isAlive).IsTrue();
            await Assert.That(checker.GetCachedStatus("127.0.0.1", port)).IsTrue();
        }
    }

    [Test]
    public async Task Non_Responding_Server_Times_Out_And_Returns_False()
    {
        using ServerStatusChecker checker = new (NullLogger<ServerStatusChecker>.Instance);

        // Bind and immediately close to get an unused port
        int unusedPort;
        using (UdpClient temp = new (new IPEndPoint(IPAddress.Loopback, 0)))
        {
            unusedPort = ((IPEndPoint)temp.Client.LocalEndPoint!).Port;
        }

        bool isAlive = await checker.CheckServerStatus("127.0.0.1", unusedPort, TimeSpan.FromMilliseconds(50));

        await Assert.That(isAlive).IsFalse();
        await Assert.That(checker.GetCachedStatus("127.0.0.1", unusedPort)).IsFalse();
    }
}

