using Hangfire;

namespace WebApplicationSample;

public sealed class RecurringJobWithError
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<RecurringJobWithError> _logger;

    public RecurringJobWithError(
        IBackgroundJobClient backgroundJobClient,
        ILogger<RecurringJobWithError> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public void Execute()
    {
        _logger.LogInformation("Executing RecurringJobWithError");
        
        _backgroundJobClient.Enqueue<RecurringJobWithError>(job => job.ExecuteNext());
        _backgroundJobClient.Enqueue<RecurringJobWithError>(job => job.ExecuteNextWithError());
    }

    public void ExecuteNext()
    {
        _logger.LogInformation("Executing Next");
    }
    
    [AutomaticRetry(Attempts = 1)]
    public void ExecuteNextWithError()
    {
        throw new Exception("Executing NextWithError");
    }
}