using System.Collections.Concurrent;

namespace Scullery;

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

    public Task<int> GetJobTotalAsync()
    {
        return Task.FromResult(_jobs.Count());
    }

    public Task<IReadOnlyList<JobDescriptor>> GetJobsAsync(int skip, int take, bool ascending = false)
    {
        IEnumerable<JobDescriptor> query = _jobs;
        if (ascending)
            query = query.OrderBy(j => int.Parse(j.Id));
        else
            query = query.OrderByDescending(j => int.Parse(j.Id));

        IReadOnlyList<JobDescriptor> list = query.Skip(skip).Take(take).ToList();

        return Task.FromResult(list);
    }

    public Task<JobDescriptor> GetJobOrDefaultAsync(string id)
    {
        if (!int.TryParse(id, out int jobId))
        {
            throw new ArgumentException("The ID must be an integral value", nameof(id));
        }

        return Task.FromResult(_jobs.Where(j => int.Parse(j.Id) == jobId).SingleOrDefault());
    }

    public Task DeleteJobAsync(string id)
    {
        if (!int.TryParse(id, out int jobId))
        {
            throw new ArgumentException("The ID must be an integral value", nameof(id));
        }

        JobDescriptor job = _jobs.Where(j => int.Parse(j.Id) == jobId).SingleOrDefault();
        if (job != null)
        {
            _jobs.Remove(job);
        }

        return Task.CompletedTask;
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
