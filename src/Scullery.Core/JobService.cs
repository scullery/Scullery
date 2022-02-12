namespace Scullery;

public class JobService : BackgroundService
{
    private readonly ILogger<JobService> _logger;
    private readonly IServiceProvider _services;
    private readonly SculleryOptions _options;

    public JobService(
        ILogger<JobService> logger,
        IOptions<SculleryOptions> optionsAccessor,
        IServiceProvider services)
    {
        _logger = logger;
        _options = optionsAccessor.Value;
        _services = services;
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job service is starting.");

        bool started = false;
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var serviceScope = _services.CreateScope())
            {
                var scopeServiceProvider = serviceScope.ServiceProvider;
                IJobStore jobStore = scopeServiceProvider.GetRequiredService<IJobStore>();
                IJobServiceEvents jobServiceEvents = scopeServiceProvider.GetRequiredService<IJobServiceEvents>();

                if (!started)
                {
                    await jobServiceEvents.OnStartedAsync();
                    started = true;
                }

                JobDescriptor job;
                try
                {
                    job = await jobStore.NextAsync(cancellationToken);
                    if (job == null)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            throw new Exception("Job store returned null for next job with no cancellation");
                        }

                        break;
                    }
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogInformation(ex, "The service was stopped while waiting for a job");
                    await jobServiceEvents.OnStoppedAsync();
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while waiting for a job");
                    await jobServiceEvents.OnFailedAsync(ex);
                    break;
                }

                try
                {
                    var jobRunner = new JobRunner(scopeServiceProvider);
                    if (_options.JobTimeout.HasValue)
                    {
                        var timeoutCts = new CancellationTokenSource(_options.JobTimeout.Value);
                        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
                        {
                            await jobRunner.RunAsync(job.Call, cancellationToken);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                _logger.LogError($"Job cancelled.");
                                await jobStore.FailedAsync(job.Id, ex: null);
                            }
                            else if (timeoutCts.IsCancellationRequested)
                            {
                                _logger.LogError($"Job timed out.");
                                await jobStore.FailedAsync(job.Id, ex: null);
                            }
                            else
                            {
                                await jobStore.SucceededAsync(job.Id);
                            }
                        }
                    }
                    else
                    {
                        await jobRunner.RunAsync(job.Call, cancellationToken);
                        await jobStore.SucceededAsync(job.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while executing job ID '{job.Id}'");

                    await jobStore.FailedAsync(job.Id, ex);
                    await jobServiceEvents.OnFailedAsync(ex);
                }
            }
        }

        _logger.LogInformation("Job service is stopping.");
    }
}
