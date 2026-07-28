using E_commerceApi.Application.Interfaces;

namespace E_commerceApi.Infrastructure.Services;

public class PendingOrderExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingOrderExpirationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _orderExpiration = TimeSpan.FromMinutes(30);

    public PendingOrderExpirationService(
        IServiceProvider serviceProvider,
        ILogger<PendingOrderExpirationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider
                    .GetRequiredService<IOrderService>();

                var cancelled = await orderService
                    .CancelExpiredOrdersAsync(_orderExpiration);

                if (cancelled > 0)
                {
                    _logger.LogInformation(
                        "Cancelled {Count} expired pending orders", cancelled);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error cancelling expired pending orders");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
