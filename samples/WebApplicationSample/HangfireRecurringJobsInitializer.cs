using Hangfire;

namespace WebApplicationSample;

public sealed class HangfireRecurringJobsInitializer : IHostedService
{
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireRecurringJobsInitializer(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _recurringJobManager.AddOrUpdate<RecurringJob>(
            "RecurringJobEveryMinute",
            job => job.Execute(),
            Cron.Minutely());
        
        _recurringJobManager.AddOrUpdate<RecurringJobWithError>(
            "RecurringJobWithErrorEveryMinute",
            job => job.Execute(),
            Cron.Minutely());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}