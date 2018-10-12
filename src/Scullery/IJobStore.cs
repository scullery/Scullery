using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scullery
{
    public enum JobStatus
    {
        Waiting,
        Ready,
        Running,
        Failed,
        Succeeded,
    }

    public class JobDescriptor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Cron { get; set; }
        public DateTime? Scheduled { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
        public JobStatus Status { get; set; }
        public JobCall Call { get; set; }
    }

    public interface IJobStore
    {
        /// <summary>
        /// Waits until a job is ready and returns it.
        /// </summary>
        /// <returns>The the descriptor of the next job. If the token is cancelled, returns nulls.</returns>
        Task<JobDescriptor> NextAsync(CancellationToken cancellationToken);

        /// <summary>
        /// If a job is available, returns it. Otherwise, returns null.
        /// </summary>
        /// <returns>The the descriptor of the next job, if available.</returns>
        Task<JobDescriptor> TryNextAsync();

        Task<string> EnqueueAsync(JobCall job);
        Task<string> ScheduleAsync(JobCall job, DateTime scheduled, TimeZoneInfo timeZone = null);
        Task RecurrentAsync(string name, string cron, JobCall job, TimeZoneInfo timeZone = null);
        //Task TriggerAsync(string name, string cron, JobCall job, TimeZoneInfo timeZone = null);
        //Task RemoveRecurrentAsync(string name);
        //Task DeleteJobAsync(string id);
        Task SucceededAsync(string id);
        Task FailedAsync(string id, Exception ex);
        Task<int> GetCountByStatusAsync(JobStatus status);
    }
}
