using Diol.applications.SignalrClient.BackgroundWorkers;
using Diol.applications.SignalrClient.Consumers;
using Diol.applications.SignalrClient.Hubs;
using Diol.Core.TraceEventProcessors;
using Diol.Share.Services;
using Microsoft.AspNetCore.SignalR;

var appOptions = new WebApplicationOptions() 
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args
};

var builder = WebApplication.CreateBuilder(appOptions);

builder.Services.AddSignalR(setting => 
{
    setting.EnableDetailedErrors = true;
});
builder.Services.AddRazorPages();
builder.Services.AddSingleton<EventPublisher>();
builder.Services.AddSingleton<SignalRConsumer>();
builder.Services.AddSingleton<DotnetProcessesService>();
builder.Services.AddHostedService<LogsBackgroundWorker>();
builder.Services.AddSingleton<BackgroundTaskQueue>(ctx =>
{
    var hubContext = ctx.GetRequiredService<IHubContext<LogsHub>>();
    return new BackgroundTaskQueue(hubContext);
});

var app = builder.Build();

app.MapHub<LogsHub>("/logsHub");
app.MapRazorPages();

app.Run();
