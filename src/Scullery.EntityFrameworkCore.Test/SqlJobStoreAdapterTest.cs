namespace Scullery.EntityFrameworkCore.Test
{
    public class SqlJobStoreAdapterTest
    {
        [Fact]
        public Task Adapter_TryClaimJob_ShouldSucceed()
        {
            return SqliteInMemoryHelper.UsingSculleryContextAsync(async context =>
            {
                var storeAdapter = new SqlJobStoreAdapter(context);
                var testStore = new EntityFrameworkJobStore(context, null, storeAdapter);
                var jobManager = new JobManager(testStore);
                var jobRunner = new JobRunner(null);

                await jobManager.EnqueueAsync(() => TestJobs.Job1(1));
                await jobManager.EnqueueAsync(() => TestJobs.Job1(2));

                Job job1 = await storeAdapter.GetCandidateJobAsync();
                Job job2 = await storeAdapter.GetCandidateJobAsync();

                bool result1 = await storeAdapter.TryClaimJobAsync(job1);
                Assert.True(result1); // Claim succeeded

                bool result2 = await storeAdapter.TryClaimJobAsync(job2);
                Assert.False(result2); // Claim failed
            });
        }
    }
}
