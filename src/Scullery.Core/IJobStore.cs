﻿namespace Scullery;

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
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? Cron { get; set; }
    public DateTime? Scheduled { get; set; }
    public TimeZoneInfo? TimeZone { get; set; }
    public JobStatus Status { get; set; }
    public JobCall Call { get; set; } = null!;
}

public interface IJobStore
{
    /// <summary>
    /// Waits until a job is ready and returns it.
    /// </summary>
    /// <returns>The the descriptor of the next job. If the token is cancelled, returns null.</returns>
    Task<JobDescriptor?> NextAsync(CancellationToken cancellationToken);

    /// <summary>
    /// If a job is available, returns it. Otherwise, returns null.
    /// </summary>
    /// <returns>The the descriptor of the next job, if available.</returns>
    Task<JobDescriptor?> TryNextAsync();

    Task<string> EnqueueAsync(JobCall job);
    Task<string> ScheduleAsync(JobCall job, DateTime scheduled, TimeZoneInfo? timeZone = null);
    Task RecurrentAsync(string name, string cron, JobCall job, TimeZoneInfo? timeZone = null);
    //Task TriggerAsync(string name, string cron, JobCall job, TimeZoneInfo? timeZone = null);
    //Task RemoveRecurrentAsync(string name);
    //Task DeleteJobAsync(string id);
    Task SucceededAsync(string id);
    Task FailedAsync(string id, Exception? ex);
    Task<int> GetCountByStatusAsync(JobStatus status);
    Task<int> GetJobTotalAsync();
    Task<IReadOnlyList<JobDescriptor>> GetJobsAsync(int skip, int take, bool ascending = false);
    Task<JobDescriptor?> GetJobOrDefaultAsync(string id);
    Task DeleteJobAsync(string id);
}
