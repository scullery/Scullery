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
        private ConcurrentQueue<JobRecord> _queue = new ConcurrentQueue<JobRecord>();
        private List<JobRecord> _jobs = new List<JobRecord>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        private static object _syncLock = new object();
        private int _idSource = 0;

        public async Task<JobRecord> NextAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _queue.TryDequeue(out JobRecord job);
            return job;
        }

        public Task<string> EnqueueAsync(JobDescriptor job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            int jobId = ++_idSource;
            var jobRecord = new JobRecord
            {
                Id = jobId.ToString(),
                Descriptor = job
            };

            lock (_syncLock)
            {
                _jobs.Add(jobRecord);
            }

            _queue.Enqueue(jobRecord);
            _signal.Release();

            return Task.FromResult(jobRecord.Id);
        }

        public Task RecurrentAsync(string name, string cron, JobDescriptor job, TimeZoneInfo timeZone = null)
        {
            return Task.CompletedTask;
        }

        public Task SucceededAsync(string id)
        {
            SetStatusById(id, JobStatus.Succeeded);
            return Task.CompletedTask;
        }

        public Task FailedAsync(string id, Exception ex)
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
                JobRecord job = _jobs.Where(j => j.Id == id).SingleOrDefault();
                if (job != null)
                    job.Status = status;
            }
        }
    }
}
