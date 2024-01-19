using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace WebApplicationSample;

public sealed class CustomTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;
    
    public CustomTelemetryProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }
    
    public void Process(ITelemetry item)
    {
        if (item is DependencyTelemetry dependency && dependency.Type.StartsWith("SQL"))
        {
            return;
        }
        
        _next.Process(item);
    }
}