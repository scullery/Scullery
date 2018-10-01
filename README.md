
# Scullery

A simple background task processor for .NET Core

## Setup

```
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        services.AddSingleton<IHostedService, ScheduleTask>();
    }
```

[Run scheduled background tasks in ASP.NET Core]: https://thinkrethink.net/2018/05/31/run-scheduled-background-tasks-in-asp-net-core/
[ASP.NET Core background processing with IHostedService]: https://thinkrethink.net/2018/02/21/asp-net-core-background-processing/
