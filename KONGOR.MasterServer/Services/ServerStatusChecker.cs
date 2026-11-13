namespace KONGOR.MasterServer.Services;

/// <summary>
///     Provides UDP ping probing to verify whether dedicated HoN game servers are active and reachable.
/// </summary>
public sealed class ServerStatusChecker(ILogger<ServerStatusChecker> logger) : IDisposable
{
    private ILogger Logger { get; } = logger;
    private readonly ConcurrentDictionary<string, bool> CachedStatus = new ();

    /// <summary>
    ///     Checks whether the game server at the specified IP address and port is responding.
    /// </summary>
    public async Task<bool> CheckServerStatus(string address, int port, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address) || port <= 0 || port > 65535)
        {
            return false;
        }

        try
        {
            using UdpClient client = new ();
            client.Client.SendTimeout = (int)timeout.TotalMilliseconds;
            client.Client.ReceiveTimeout = (int)timeout.TotalMilliseconds;

            IPAddress ip = IPAddress.Parse(address);
            IPEndPoint endPoint = new (ip, port);

            // HoN Server Status Ping Probe (8-byte header standard probe)
            byte[] probePacket = [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

            await client.SendAsync(probePacket, probePacket.Length, endPoint);

            using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            UdpReceiveResult result = await client.ReceiveAsync(timeoutCts.Token);
            bool isSuccess = result.Buffer.Length > 0;

            CachedStatus[$"{address}:{port}"] = isSuccess;
            return isSuccess;
        }
        catch (OperationCanceledException)
        {
            CachedStatus[$"{address}:{port}"] = false;
            return false;
        }
        catch (Exception exception)
        {
            Logger.LogDebug(@"UDP Ping To {Address}:{Port} Failed: {Message}", address, port, exception.Message);
            CachedStatus[$"{address}:{port}"] = false;
            return false;
        }
    }

    /// <summary>
    ///     Returns the last known cached status for the specified server, or queries if not cached.
    /// </summary>
    public bool GetCachedStatus(string address, int port)
    {
        return CachedStatus.TryGetValue($"{address}:{port}", out bool status) && status;
    }

    public void Dispose()
    {
        CachedStatus.Clear();
    }
}

