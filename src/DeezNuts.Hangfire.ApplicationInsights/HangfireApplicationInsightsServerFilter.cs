using Hangfire.Server;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeezNuts.Hangfire.ApplicationInsights;

public sealed class HangfireApplicationInsightsServerFilter : IServerFilter
{
    private const string OperationItem = "Telemetry.Operation";
    
    private readonly IOptions<HangfireApplicationInsightsOptions> _options;
    private readonly TelemetryClient _telemetryClient;

    public HangfireApplicationInsightsServerFilter(
        IOptions<HangfireApplicationInsightsOptions> options,
        TelemetryClient telemetryClient)
    {
        _options = options;
        _telemetryClient = telemetryClient;
    }

    public void OnPerforming(PerformingContext context)
    {
        var operation = _telemetryClient.StartOperation<RequestTelemetry>(
            $"Job {context.BackgroundJob.Job.Type.Name}.{context.BackgroundJob.Job.Method.Name}",
            context.GetJobParameter<string>("Operation.RootId"),
            context.GetJobParameter<string>("Operation.ParentId"));
        
        operation.Telemetry.Properties.Add("JobId", context.BackgroundJob.Id);
        operation.Telemetry.Properties.Add("JobType", context.BackgroundJob.Job.Type.FullName);
        operation.Telemetry.Properties.Add("JobMethod", context.BackgroundJob.Job.Method.Name);
        operation.Telemetry.Properties.Add("JobCreatedAt", context.BackgroundJob.CreatedAt.ToString("O"));

        if (_options.Value.SerializeJobArguments)
        {
            try
            {
                operation.Telemetry.Properties.Add("JobArguments", JsonConvert.SerializeObject(
                    context.BackgroundJob.Job.Args?.Where(c => c is not CancellationToken)));
            }
            catch
            {
                operation.Telemetry.Properties.Add("JobArguments", "Serialization failed");
            }
        }

        context.Items[OperationItem] = operation;
        operation.Telemetry.Start();
    }

    public void OnPerformed(PerformedContext context)
    {
        var operation = GetOperation(context);
        if (operation is null)
        {
            return;
        }

        if (context.Exception is null || context.ExceptionHandled)
        {
            operation.Telemetry.Success = true;
            operation.Telemetry.ResponseCode = "Succeeded";
        }
        else
        {
            operation.Telemetry.Success = false;
            operation.Telemetry.ResponseCode = "Failed";

            var exceptionTelemetry = new ExceptionTelemetry(context.Exception);
            exceptionTelemetry.Context.Operation.Id = operation.Telemetry.Context.Operation.Id;
            exceptionTelemetry.Context.Operation.ParentId = operation.Telemetry.Id;
            
            _telemetryClient.TrackException(exceptionTelemetry);
        }
        
        operation.Telemetry.Stop();
        operation.Dispose();
    }
    
    private static IOperationHolder<RequestTelemetry>? GetOperation(PerformContext context)
    {
        context.Items.TryGetValue(OperationItem, out var item);
        return item as IOperationHolder<RequestTelemetry>;
    }
}