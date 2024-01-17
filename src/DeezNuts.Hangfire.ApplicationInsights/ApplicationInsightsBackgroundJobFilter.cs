using Hangfire.Client;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;

namespace DeezNuts.Hangfire.ApplicationInsights;

public sealed class ApplicationInsightsBackgroundJobFilter : IClientFilter, IClientExceptionFilter
{
    private const string OperationItem = "Telemetry.Operation";

    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightsBackgroundJobFilter(TelemetryClient telemetryClient)
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
        
        operation.Telemetry.Properties.Add("JobId", context.BackgroundJob.Id);
        operation.Telemetry.Properties.Add("JobCreatedAt", context.BackgroundJob.CreatedAt.ToString("O"));
        operation.Telemetry.Success = true;
        operation.Telemetry.ResultCode = "Enqueued";
        operation.Telemetry.Stop();
        context.Items.Remove(OperationItem);
        operation.Dispose();
    }

    public void OnClientException(ClientExceptionContext filterContext)
    {
        var operation = GetOperation(filterContext);
        if (operation is null)
        {
            return;
        }

        operation.Telemetry.Success = false;
        operation.Telemetry.ResultCode = "Failed";
        operation.Telemetry.Stop();
        filterContext.Items.Remove(OperationItem);
        operation.Dispose();
    }
    
    private static IOperationHolder<DependencyTelemetry>? GetOperation(CreateContext context)
    {
        context.Items.TryGetValue(OperationItem, out var item);
        return item as IOperationHolder<DependencyTelemetry>;
    }
}