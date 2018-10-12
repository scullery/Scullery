using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Scullery.EntityFrameworkCore
{
    public class SqlJobStoreQueueTest
    {
        [Fact]
        public Task Queue_TryClaimJob_ShouldSucceed()
        {
            return SqliteInMemoryHelper.UsingSculleryContextAsync(async context =>
            {
                var storeQueue = new SqlJobStoreQueue(context);
                var testStore = new EntityFrameworkJobStore(context, null, storeQueue);
                var jobManager = new JobManager(testStore);
                var jobRunner = new JobRunner(null);

                await jobManager.EnqueueAsync(() => TestJobs.Job1(1));
                await jobManager.EnqueueAsync(() => TestJobs.Job1(2));

                Job job1 = await storeQueue.GetCandidateJobAsync();
                Job job2 = await storeQueue.GetCandidateJobAsync();

                bool result1 = await storeQueue.TryClaimJobAsync(job1);
                Assert.True(result1); // Claim succeeded

                bool result2 = await storeQueue.TryClaimJobAsync(job2);
                Assert.False(result2); // Claim failed
            });
        }
    }
}
