using Hangfire.Extensions.Diagnostics.ApplicationInsights;
using Hangfire.SqlServer;
using Hangfire;
using WebApplicationSample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddHangfire((serviceProvider, globalConfiguration) => globalConfiguration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetHangfireConnectionString(), new SqlServerStorageOptions
    {
        QueuePollInterval = TimeSpan.Zero,
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    })
    .UseApplicationInsights(serviceProvider));

builder.Services.AddHangfireServer();
builder.Services.AddHangfireApplicationInsights();

builder.Services.AddHostedService<HangfireRecurringJobsInitializer>();

var app = builder.Build();

app.MapHangfireDashboard();

app.Run();