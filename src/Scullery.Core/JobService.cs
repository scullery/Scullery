using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Scullery
{
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

            while (!cancellationToken.IsCancellationRequested)
            {
                using (var serviceScope = _services.CreateScope())
                {
                    var scopeServiceProvider = serviceScope.ServiceProvider;
                    IJobStore jobStore = scopeServiceProvider.GetRequiredService<IJobStore>();

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
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error occurred loading job.");

                        IJobServiceEvents jobServiceEvents = scopeServiceProvider.GetRequiredService<IJobServiceEvents>();
                        await jobServiceEvents.OnStoppedAsync($"Error occurred loading job.", ex);

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
                        _logger.LogError(ex, $"Error occurred executing job.");

                        await jobStore.FailedAsync(job.Id, ex);
                    }
                }
            }

            _logger.LogInformation("Job service is stopping.");
        }
    }
}
