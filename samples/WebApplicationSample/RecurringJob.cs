using Hangfire;

namespace WebApplicationSample;

public sealed class RecurringJob
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<RecurringJob> _logger;

    public RecurringJob(
        IBackgroundJobClient backgroundJobClient,
        ILogger<RecurringJob> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public void Execute()
    {
        _logger.LogInformation("Executing RecurringJob");
        
        _backgroundJobClient.Enqueue<RecurringJob>(job => job.ExecuteNext());
        _backgroundJobClient.Enqueue<RecurringJob>(job => job.ExecuteNextWithParams("Hello World"));
    }

    public void ExecuteNext()
    {
        _logger.LogInformation("Executing Next");
    }

    public void ExecuteNextWithParams(string message)
    {
        _logger.LogInformation("Executing NextWithParams {Message}", message);
    }
}