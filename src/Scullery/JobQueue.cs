using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scullery
{
#if false
    public class JobQueue : IJobQueue
    {
        private readonly IJobStore _jobStore;
        private readonly IJobRunner _jobRunner;

        public JobQueue(
            IJobStore jobStore,
            IJobRunner jobRunner)
        {
            _jobStore = jobStore;
            _jobRunner = jobRunner;
        }

        public async Task<(string id, Func<CancellationToken, Task> job)> DequeueAsync(CancellationToken cancellationToken)
        {
            JobRecord job = await _jobStore.NextAsync(cancellationToken);
            if (job == null)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new Exception("Job store returned null for next job with no cancellation");
                }

                // If token is cancelled, return a completed task.
                return (null, (token) => Task.CompletedTask);
            }

            return (job.Id, (token) => _jobRunner.RunAsync(job.Descriptor, token));
        }

        public Task SucceededAsync(string id)
        {
            return Task.CompletedTask;
        }

        public Task FailedAsync(string id, Exception ex)
        {
            return Task.CompletedTask;
        }
    }
#endif
}
