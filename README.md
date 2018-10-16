
# Scullery

A simple background task processor for .NET Core

## Background

Web applications often require background processing capabilities.
This typically requires setting up a separate process to run these tasks.
Recent capabilities introduced into ASP<span>.</span>NET Core allow for background processing within the web application itself.
This project attempts to enable such processing using these capabilities.

## Setup

First, call `AddScullery` from `Startup.cs`.

```
using Scullery;

    ...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        services.AddScullery(options =>
        {
            options.InMemoryStore = true;
        });
    }
```

The `InMemoryStore` option can be used to get started, but it's not production ready.
The `AddScullery` extension method adds the `JobService` class, among other things.

```
    services.AddSingleton<IHostedService, JobService>();
```

The `JobService` runs jobs in the background using the new [hosted service][Background tasks with hosted services in ASP.NET Core] capabilities provided in version 2.1 of ASP<span>.</span>NET Core.

## Usage

While the `JobService` runs the jobs in the background, jobs are added using the `JobManager` class.
This is made available using normal dependency injection mechanisms, as in this example from an MVC controller.

```
using Scullery;

    ...

    private readonly JobManager _jobManager;

    public HomeController(JobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public async Task<IActionResult> Index())
    {
        await _jobManager.EnqueueAsync(() => BackgroundMethod());
        return View();
    }

    public static void BackgroundMethod()
    {
    }
```

Jobs are defined as methods.
They can be static methods, as above, or instance methods.
Instance methods must specify their class as a generic type argument.

```
    await jobManager.EnqueueAsync<MyJobs>(s => s.BackgroundInstanceMethodAsync("Hello!"));
```

Instance methods can be used if your class needs access to services.
Background methods can also be made `async` by returning `Task` rather than `void`.

```
    public class MyJobs
    {
        private readonly IService _service;
        
        public MyJobs(IService service)
        {
            _service = service;
        }

        public async Task BackgroundInstanceMethodAsync(string message)
        {
            await _service.DoBackgroundWorkAsync(message);
        }
    }
```

[//]: # (References)

[Background tasks with hosted services in ASP.NET Core]: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.1
[ASP.NET Core background processing with IHostedService]: https://thinkrethink.net/2018/02/21/asp-net-core-background-processing/
[Run scheduled background tasks in ASP.NET Core]: https://thinkrethink.net/2018/05/31/run-scheduled-background-tasks-in-asp-net-core/
