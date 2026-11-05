using Leia.Application.Interfaces;

namespace Leia.Api.BackgroundServices
{
    public class OrderTimeoutWorker(IServiceScopeFactory scopeFactory, ILogger<OrderTimeoutWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<IOrderTimeoutService>();
                        var count = await service.CancelExpiredAsync();

                        if (count > 0)
                            logger.LogInformation("{Count} expired orders were cancelled.", count);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogError(ex, "Order timeout check failed.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}