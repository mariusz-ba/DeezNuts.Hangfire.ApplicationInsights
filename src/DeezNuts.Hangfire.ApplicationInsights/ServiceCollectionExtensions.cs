using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace DeezNuts.Hangfire.ApplicationInsights;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds necessary Hangfire filters to service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddHangfireApplicationInsights(this IServiceCollection services)
        => services.AddHangfireApplicationInsights(_ => { });
    
    /// <summary>
    /// Adds necessary Hangfire filters to service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="HangfireApplicationInsightsOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddHangfireApplicationInsights(
        this IServiceCollection services,
        Action<HangfireApplicationInsightsOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddSingleton<HangfireApplicationInsightsClientFilter>();
        services.AddSingleton<HangfireApplicationInsightsServerFilter>();

        return services;
    }

    /// <summary>
    /// Applies Hangfire job filters responsible for proper telemetry tracking.
    /// </summary>
    /// <param name="configuration">The <see cref="IGlobalConfiguration"/> instance.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance used to resolve filters.</param>
    /// <returns>The <see cref="IGlobalConfiguration"/>.</returns>
    public static IGlobalConfiguration UseApplicationInsights(
        this IGlobalConfiguration configuration, IServiceProvider serviceProvider)
        => configuration
            .UseFilter(serviceProvider.GetRequiredService<HangfireApplicationInsightsClientFilter>())
            .UseFilter(serviceProvider.GetRequiredService<HangfireApplicationInsightsServerFilter>());
}