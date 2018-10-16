using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Scullery
{
    public class JobService : BackgroundService
    {
        private readonly ILogger<JobService> _logger;
        private readonly IServiceProvider _services;

        public JobService(
            ILogger<JobService> logger,
            IServiceProvider services)
        {
            _logger = logger;
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
                        break;
                    }

                    try
                    {
                        var jobRunner = new JobRunner(scopeServiceProvider);
                        await jobRunner.RunAsync(job.Call, cancellationToken);

                        await jobStore.SucceededAsync(job.Id);
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
