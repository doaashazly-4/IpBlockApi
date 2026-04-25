using IpBlockApi.Repositories;

namespace IpBlockApi.Background;

public sealed class TemporalBlockCleanupBackgroundService : BackgroundService
{
    private readonly ITemporalBlockRepository _temporalBlocks;
    private readonly ILogger<TemporalBlockCleanupBackgroundService> _logger;

    public TemporalBlockCleanupBackgroundService(
        ITemporalBlockRepository temporalBlocks,
        ILogger<TemporalBlockCleanupBackgroundService> logger)
    {
        _temporalBlocks = temporalBlocks;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                _temporalBlocks.RemoveExpired();
                _logger.LogDebug("Temporal block cleanup completed.");
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // shutdown
        }
    }
}
