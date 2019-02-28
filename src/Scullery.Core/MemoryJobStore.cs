using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scullery
{
    public class MemoryJobStore : IJobStore
    {
        private ConcurrentQueue<JobDescriptor> _queue = new ConcurrentQueue<JobDescriptor>();
        private List<JobDescriptor> _jobs = new List<JobDescriptor>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        private static object _syncLock = new object();
        private int _idSource = 0;

        public Task<JobDescriptor> TryNextAsync()
        {
            JobDescriptor waiting;
            lock (_syncLock)
            {
                waiting = _jobs
                    .Where(d => d.Status == JobStatus.Waiting)
                    .OrderBy(d => d.Scheduled)
                    .FirstOrDefault();
            }

            if (waiting != null)
            {
                if (!waiting.Scheduled.HasValue)
                    throw new InvalidOperationException("A scheduled job must have a time");

                // Convert the scheduled time to UTC for comparison. If 
                // TimeZone is null, defaults to UTC.
                DateTime scheduled = waiting.TimeZone == null
                    ? waiting.Scheduled.Value
                    : TimeZoneInfo.ConvertTimeToUtc(waiting.Scheduled.Value, waiting.TimeZone);

                DateTime now = DateTime.UtcNow;
                if (scheduled <= now)
                {
                    // This one is ready now. 
                    _queue.Enqueue(waiting);
                }
            }

            JobDescriptor job;
            _queue.TryDequeue(out job);

            return Task.FromResult(job);
        }

        public async Task<JobDescriptor> NextAsync(CancellationToken cancellationToken)
        {
            JobDescriptor job;
            do
            {
                JobDescriptor waiting;
                lock (_syncLock)
                {
                    waiting = _jobs
                        .Where(d => d.Status == JobStatus.Waiting)
                        .OrderBy(d => d.Scheduled)
                        .FirstOrDefault();
                }

                if (waiting != null)
                {
                    if (!waiting.Scheduled.HasValue)
                        throw new InvalidOperationException("A scheduled job must have a time");

                    // Convert the scheduled time to UTC for comparison. If 
                    // TimeZone is null, defaults to UTC.
                    DateTime scheduled = waiting.TimeZone == null
                        ? waiting.Scheduled.Value
                        : TimeZoneInfo.ConvertTimeToUtc(waiting.Scheduled.Value, waiting.TimeZone);

                    DateTime now = DateTime.UtcNow;
                    if (scheduled <= now)
                    {
                        // This one is ready now. 
                        _queue.Enqueue(waiting);
                    }
                    else
                    {
                        // Wake when due.
                        TimeSpan timeout = now - scheduled;
                        bool signaled = await _signal.WaitAsync(timeout, cancellationToken);
                        if (!signaled)
                            _queue.Enqueue(waiting);
                    }
                }
                else
                {
                    await _signal.WaitAsync(cancellationToken);
                }

                _queue.TryDequeue(out job);
            } while (job == null && !cancellationToken.IsCancellationRequested);

            return job;
        }

        public Task<string> EnqueueAsync(JobCall job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            JobDescriptor jobDescriptor = Enqueue(job, JobStatus.Ready, null);

            _queue.Enqueue(jobDescriptor);
            _signal.Release();

            return Task.FromResult(jobDescriptor.Id);
        }

        public Task<string> ScheduleAsync(JobCall job, DateTime scheduled, TimeZoneInfo timeZone = null)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            JobDescriptor jobDescriptor = Enqueue(job, JobStatus.Waiting, scheduled, timeZone);

            // Refresh the queue observer.
            _signal.Release();

            return Task.FromResult(jobDescriptor.Id);
        }

        private JobDescriptor Enqueue(JobCall job, JobStatus status, DateTime? scheduled, TimeZoneInfo timeZone = null)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (status == JobStatus.Waiting && !scheduled.HasValue)
            {
                throw new ArgumentNullException(nameof(scheduled));
            }

            int jobId = ++_idSource;
            var jobDescriptor = new JobDescriptor
            {
                Id = jobId.ToString(),
                Status = status,
                Scheduled = scheduled,
                TimeZone = timeZone,
                Call = job
            };

            lock (_syncLock)
            {
                _jobs.Add(jobDescriptor);
            }

            return jobDescriptor;
        }

        public Task RecurrentAsync(string name, string cron, JobCall job, TimeZoneInfo timeZone = null)
        {
            return Task.CompletedTask;
        }

        public Task SucceededAsync(string id)
        {
            SetStatusById(id, JobStatus.Succeeded);
            return Task.CompletedTask;
        }

        public Task FailedAsync(string id, Exception ex = null)
        {
            // TODO: Store exception
            SetStatusById(id, JobStatus.Failed);
            return Task.CompletedTask;
        }

        public Task<int> GetCountByStatusAsync(JobStatus status)
        {
            lock (_syncLock)
            {
                return Task.FromResult(_jobs.Where(j => j.Status == status).Count());
            }
        }

        private void SetStatusById(string id, JobStatus status)
        {
            lock (_syncLock)
            {
                JobDescriptor job = _jobs.Where(j => j.Id == id).SingleOrDefault();
                if (job != null)
                    job.Status = status;
            }
        }
    }
}
