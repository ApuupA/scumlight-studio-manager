using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.API.BackgroundJobs
{
    /// <summary>
    /// Writes the finance archives every night at 2:30 - before the backup at 3:00 (scripts/backup.sh),
    /// so every backup contains a current, readable archive. Also runs a minute after the start.
    /// </summary>
    public class FinanceArchiveJob : BackgroundService
    {
        private static readonly TimeOnly RunAt = new(2, 30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FinanceArchiveJob> _logger;

        public FinanceArchiveJob(IServiceScopeFactory scopeFactory, ILogger<FinanceArchiveJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Not right at the start - the database migrations and the first requests come first
            var delay = TimeSpan.FromMinutes(1);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    return; // the app is shutting down
                }

                await WriteArchives();
                delay = TimeUntilNextRun();
            }
        }

        private async Task WriteArchives()
        {
            try
            {
                // The services are "scoped" (one database context per request) - a background job creates its own scope
                using var scope = _scopeFactory.CreateScope();
                var archiveService = scope.ServiceProvider.GetRequiredService<IFinanceArchiveService>();
                await archiveService.WriteCurrentArchives();
                _logger.LogInformation("Finance archives written");
            }
            catch (Exception exception)
            {
                // Never stop the app because of the archive - try again the next night
                _logger.LogError(exception, "Writing the finance archives failed");
            }
        }

        private static TimeSpan TimeUntilNextRun()
        {
            var now = DateTime.Now;
            var next = now.Date + RunAt.ToTimeSpan();
            if (next <= now)
            {
                next = next.AddDays(1);
            }
            return next - now;
        }
    }
}
