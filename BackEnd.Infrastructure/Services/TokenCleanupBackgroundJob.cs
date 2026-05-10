using BackEnd.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackEnd.Infrastructure.Services
{
    public class TokenCleanupBackgroundJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupBackgroundJob> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(7); // Run once a week
        private readonly int _inactiveMonths = 6; // Cleanup tokens older than 6 months

        public TokenCleanupBackgroundJob(IServiceProvider serviceProvider, ILogger<TokenCleanupBackgroundJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Background Job is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Token Cleanup Background Job is working.");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tokenRepo = scope.ServiceProvider.GetRequiredService<IDeviceTokenRepository>();
                        var olderThan = DateTime.UtcNow.AddMonths(-_inactiveMonths);

                        await tokenRepo.RemoveInactiveTokensAsync(olderThan, stoppingToken);
                        await tokenRepo.SaveChangesAsync(stoppingToken);
                    }

                    _logger.LogInformation("Token Cleanup Background Job finished successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Token Cleanup Background Job.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Token Cleanup Background Job is stopping.");
        }
    }
}
