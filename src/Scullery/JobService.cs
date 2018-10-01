using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Scullery
{
    public interface IJobQueue
    {
        Task<(string id, Func<CancellationToken, Task> job)> DequeueAsync(CancellationToken cancellationToken);
        Task SucceededAsync(string id);
        Task FailedAsync(string id, Exception ex);
    }

    public class JobService : BackgroundService
    {
        private readonly ILogger<JobService> _logger;
        //private readonly IJobQueue _queue;
        private readonly IJobStore _jobStore;
        private readonly IJobRunner _jobRunner;

        public JobService(
            //ILoggerFactory loggerFactory,
            ILogger<JobService> logger,
            IJobStore jobStore,
            IJobRunner jobRunner)
        {
            //_logger = loggerFactory.CreateLogger<JobService>();
            _logger = logger;
            _jobStore = jobStore;
            _jobRunner = jobRunner;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Job service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                JobRecord job = await _jobStore.NextAsync(cancellationToken);
                if (job == null)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw new Exception("Job store returned null for next job with no cancellation");
                    }

                    break;
                }

                try
                {
                    await _jobRunner.RunAsync(job.Descriptor, cancellationToken);

                    await _jobStore.SucceededAsync(job.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred executing job.");

                    await _jobStore.FailedAsync(job.Id, ex);
                }
            }

            _logger.LogInformation("Job service is stopping.");
        }
    }
}
