using Hangfire.Client;
using Hangfire.Server;
using Hangfire.States;
using Hangfire;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        
        // Default implementations have to be registered in order to add custom performer.
        // https://github.com/HangfireIO/Hangfire/issues/1375#issuecomment-475641751
        services.TryAddSingleton<IBackgroundJobFactory, BackgroundJobFactory>();
        services.TryAddSingleton<IBackgroundJobStateChanger, BackgroundJobStateChanger>();
        services.TryAddSingleton<IBackgroundJobPerformer, BackgroundJobPerformer>();

        services.AddSingleton<ApplicationInsightsBackgroundJobFilter>();

        services.Decorate<IBackgroundJobPerformer, ApplicationInsightsBackgroundJobPerformer>();

        return services;
    }

    public static IGlobalConfiguration<ApplicationInsightsBackgroundJobFilter> UseApplicationInsights(
        this IGlobalConfiguration configuration, IServiceProvider serviceProvider)
        => configuration.UseFilter(serviceProvider.GetRequiredService<ApplicationInsightsBackgroundJobFilter>());
}