using System.Threading.Channels;

namespace KONGOR.MasterServer.Services;

public sealed class GeoInformationResolver(IServiceScopeFactory serviceScopeFactory, ILogger<GeoInformationResolver> logger) : BackgroundService
{
    private IServiceScopeFactory ServiceScopeFactory { get; } = serviceScopeFactory;
    private ILogger Logger { get; } = logger;

    private static readonly Channel<string> IPQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
    {
        SingleReader = true
    });

    public static void EnqueueIPAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Equals("127.0.0.1") || ipAddress.Equals("::1"))
        {
            return;
        }

        IPQueue.Writer.TryWrite(ipAddress);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("GeoInformationResolver background worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await IPQueue.Reader.WaitToReadAsync(stoppingToken))
                {
                    while (IPQueue.Reader.TryRead(out string? ipAddress))
                    {
                        if (string.IsNullOrWhiteSpace(ipAddress))
                        {
                            continue;
                        }

                        using IServiceScope scope = ServiceScopeFactory.CreateScope();
                        MerrickContext merrickContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

                        bool exists = await merrickContext.GeoInformations
                            .AnyAsync(candidate => candidate.IPAddress.Equals(ipAddress), stoppingToken);

                        if (exists.Equals(false))
                        {
                            // Create standard placeholder or geocoded entry
                            GeoInformation entry = new ()
                            {
                                IPAddress = ipAddress,
                                Latitude = 0.0f,
                                Longitude = 0.0f,
                                TimeZone = "UTC"
                            };

                            await merrickContext.GeoInformations.AddAsync(entry, stoppingToken);
                            await merrickContext.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "Error resolving IP address in GeoInformationResolver");
            }
        }
    }
}

