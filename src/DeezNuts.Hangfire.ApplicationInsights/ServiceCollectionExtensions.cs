using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace DeezNuts.Hangfire.ApplicationInsights;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHangfireApplicationInsights(this IServiceCollection services)
        => services.AddHangfireApplicationInsights(_ => { });
    
    public static IServiceCollection AddHangfireApplicationInsights(
        this IServiceCollection services,
        Action<HangfireApplicationInsightsOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddSingleton<HangfireApplicationInsightsClientFilter>();
        services.AddSingleton<HangfireApplicationInsightsServerFilter>();

        return services;
    }

    public static IGlobalConfiguration UseApplicationInsights(
        this IGlobalConfiguration configuration, IServiceProvider serviceProvider)
        => configuration
            .UseFilter(serviceProvider.GetRequiredService<HangfireApplicationInsightsClientFilter>())
            .UseFilter(serviceProvider.GetRequiredService<HangfireApplicationInsightsServerFilter>());
}