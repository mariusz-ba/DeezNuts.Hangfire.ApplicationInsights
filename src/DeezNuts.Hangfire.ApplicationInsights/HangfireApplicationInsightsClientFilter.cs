using Hangfire.Client;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;

namespace DeezNuts.Hangfire.ApplicationInsights;

public sealed class HangfireApplicationInsightsClientFilter : IClientFilter
{
    private const string OperationItem = "Telemetry.Operation";

    private readonly TelemetryClient _telemetryClient;

    public HangfireApplicationInsightsClientFilter(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void OnCreating(CreatingContext context)
    {
        var operation = _telemetryClient.StartOperation<DependencyTelemetry>(
            $"Enqueue {context.Job.Type.Name}.{context.Job.Method.Name}");
        
        context.SetJobParameter("Operation.RootId", operation.Telemetry.Context.Operation.Id);
        context.SetJobParameter("Operation.ParentId", operation.Telemetry.Id);

        operation.Telemetry.Type = "Hangfire";
        operation.Telemetry.Properties.Add("JobType", context.Job.Type.FullName);
        operation.Telemetry.Properties.Add("JobMethod", context.Job.Method.Name);
        context.Items[OperationItem] = operation;
        operation.Telemetry.Start();
    }

    public void OnCreated(CreatedContext context)
    {
        var operation = GetOperation(context);
        if (operation is null)
        {
            return;
        }

        if (context.Exception is null || context.ExceptionHandled)
        {
            operation.Telemetry.Properties.Add("JobId", context.BackgroundJob.Id);
            operation.Telemetry.Properties.Add("JobCreatedAt", context.BackgroundJob.CreatedAt.ToString("O"));
            operation.Telemetry.Success = true;
            operation.Telemetry.ResultCode = "Enqueued";
        }
        else
        {
            operation.Telemetry.Success = false;
            operation.Telemetry.ResultCode = "Failed";
            
            var exceptionTelemetry = new ExceptionTelemetry(context.Exception);
            exceptionTelemetry.Context.Operation.Id = operation.Telemetry.Context.Operation.Id;
            exceptionTelemetry.Context.Operation.ParentId = operation.Telemetry.Id;
            
            _telemetryClient.TrackException(exceptionTelemetry);
        }
        
        operation.Telemetry.Stop();
        operation.Dispose();
    }
    
    private static IOperationHolder<DependencyTelemetry>? GetOperation(CreateContext context)
    {
        context.Items.TryGetValue(OperationItem, out var item);
        return item as IOperationHolder<DependencyTelemetry>;
    }
}