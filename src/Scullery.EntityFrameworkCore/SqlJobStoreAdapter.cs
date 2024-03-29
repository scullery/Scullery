﻿namespace Scullery.EntityFrameworkCore;

/// <summary>
/// Adapts the job store to use standard SQL for pulling jobs from the queue.
/// This is the default adapter if one is not specified.
/// </summary>
class SqlJobStoreAdapter : IEntityFrameworkJobStoreAdapter
{
    public readonly SculleryContext _context;

    public SqlJobStoreAdapter(SculleryContext context)
    {
        _context = context;
    }

    public async Task<Job?> TryNextAsync()
    {
        Job? job;

        do
        {
            job = await GetCandidateJobAsync();
            if (job == null)
            {
                break;
            }
        } while (!await TryClaimJobAsync(job));

        return job;
    }

    public Task<Job?> GetCandidateJobAsync()
    {
        return _context.Jobs.Where(j => j.Status == JobStatus.Ready).OrderBy(j => j.Scheduled).FirstOrDefaultAsync();
    }

    public async Task<bool> TryClaimJobAsync(Job job)
    {
        int result;
        if (_context.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            // Quote identifiers to prevent case folding on PG.
            result = await _context.Database.ExecuteSqlInterpolatedAsync($@"UPDATE ""Jobs"" 
SET ""Status"" = {(int)JobStatus.Running}
WHERE ""Id"" = {job.Id} AND ""Status"" = {(int)JobStatus.Ready}");
        }
        else
        {
            result = await _context.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Jobs 
SET Status = {(int)JobStatus.Running}
WHERE Id = {job.Id} AND Status = {(int)JobStatus.Ready}");
        }

        return result == 1;
    }
}
