using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Scullery.EntityFrameworkCore
{
    public class SqlJobStoreQueue : IEntityFrameworkJobStoreQueue
    {
        public readonly SculleryContext _context;

        public SqlJobStoreQueue(SculleryContext context)
        {
            _context = context;
        }

        public async Task<Job> TryNextAsync()
        {
            Job job;

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

        public Task<Job> GetCandidateJobAsync()
        {
            return _context.Jobs.Where(j => j.Status == JobStatus.Ready).OrderBy(j => j.Scheduled).FirstOrDefaultAsync();
        }

        public async Task<bool> TryClaimJobAsync(Job job)
        {
            int result = await _context.Database.ExecuteSqlCommandAsync($@"
UPDATE Jobs 
SET Status = {(int)JobStatus.Running}
WHERE Id = {job.Id} AND Status = {(int)JobStatus.Ready}");
            return result == 1;
        }
    }
}
