using Hangfire.Server;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DeezNuts.Hangfire.ApplicationInsights;

public sealed class ApplicationInsightsBackgroundJobPerformer : IBackgroundJobPerformer
{
    private readonly IOptions<HangfireApplicationInsightsOptions> _options;
    private readonly IBackgroundJobPerformer _performer;
    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightsBackgroundJobPerformer(
        IOptions<HangfireApplicationInsightsOptions> options,
        IBackgroundJobPerformer performer,
        TelemetryClient telemetryClient)
    {
        _options = options;
        _performer = performer;
        _telemetryClient = telemetryClient;
    }

    public object Perform(PerformContext context)
    {
        using var operation = _telemetryClient.StartOperation<RequestTelemetry>(
            $"Job {context.BackgroundJob.Job.Type.Name}.{context.BackgroundJob.Job.Method.Name}",
            context.GetJobParameter<string>("Operation.RootId"),
            context.GetJobParameter<string>("Operation.ParentId"));

        try
        {
            operation.Telemetry.Properties.Add("JobId", context.BackgroundJob.Id);
            operation.Telemetry.Properties.Add("JobType", context.BackgroundJob.Job.Type.FullName);
            operation.Telemetry.Properties.Add("JobMethod", context.BackgroundJob.Job.Method.Name);
            operation.Telemetry.Properties.Add("JobCreatedAt", context.BackgroundJob.CreatedAt.ToString("O"));

            if (_options.Value.SerializeJobArguments)
            {
                try
                {
                    operation.Telemetry.Properties.Add("JobArguments", JsonSerializer.Serialize(
                        context.BackgroundJob.Job.Args?.Where(c => c is not CancellationToken)));
                }
                catch
                {
                    operation.Telemetry.Properties.Add("JobArguments", "Serialization failed");
                }
            }

            var result = _performer.Perform(context);

            operation.Telemetry.Success = true;
            operation.Telemetry.ResponseCode = "Succeeded";
            
            return result;
        }
        catch (Exception exception)
        {
            operation.Telemetry.Success = false;
            operation.Telemetry.ResponseCode = "Failed";
            _telemetryClient.TrackException(exception);
            throw;
        }
    }
}