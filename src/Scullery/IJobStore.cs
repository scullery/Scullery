using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scullery
{
    public class JobRecord
    {
        public string Id { get; set; }
        public JobDescriptor Descriptor { get; set; }
    }

    public interface IJobStore
    {
        /// <summary>
        /// Waits until a job is ready and returns it.
        /// </summary>
        /// <returns>The ID and the descriptor of the next job. If the token is cancelled, returns nulls.</returns>
        Task<JobRecord> NextAsync(CancellationToken cancellationToken);

        Task<string> EnqueueAsync(JobDescriptor job);
        Task SucceededAsync(string id);
        Task FailedAsync(string id, Exception ex);
    }
}
