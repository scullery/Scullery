namespace Scullery.EntityFrameworkCore.Test;

public class EntityFrameworkJobStoreTest
{
    [Fact]
    public Task JobStore_TryNext_ShouldSucceed()
    {
        return SqliteInMemoryHelper.UsingSculleryContextAsync(async context =>
        {
            var jobStore = new EntityFrameworkJobStore(context);
            var jobManager = new JobManager(jobStore);
            var jobRunner = new JobRunner(null!);

            await jobManager.EnqueueAsync(() => TestJobs.Job1(1));
            await jobManager.EnqueueAsync(() => TestJobs.Job1(2));

            JobDescriptor? job1 = await jobStore.TryNextAsync();
            Assert.NotNull(job1);
            Assert.Equal("1", job1!.Id);

            JobDescriptor? job2 = await jobStore.TryNextAsync();
            Assert.NotNull(job2);
            Assert.Equal("2", job2!.Id);

            JobDescriptor? job3 = await jobStore.TryNextAsync();
            Assert.Null(job3);
        });
    }

    [Fact]
    public Task JobStore_Next_ShouldSucceed()
    {
        return SqliteInMemoryHelper.UsingSculleryContextAsync(async context =>
        {
            var jobStore = new EntityFrameworkJobStore(context);
            var jobManager = new JobManager(jobStore);
            var jobRunner = new JobRunner(null!);

            await jobManager.EnqueueAsync(() => TestJobs.Job1(1));
            await jobManager.EnqueueAsync(() => TestJobs.Job1(2));

            var tokenSource = new CancellationTokenSource();

            JobDescriptor? job1 = await jobStore.NextAsync(tokenSource.Token);
            Assert.NotNull(job1);
            Assert.Equal("1", job1!.Id);

            JobDescriptor? job2 = await jobStore.NextAsync(tokenSource.Token);
            Assert.NotNull(job2);
            Assert.Equal("2", job2!.Id);

            tokenSource.Cancel();

            JobDescriptor? job3 = await jobStore.NextAsync(tokenSource.Token);
            Assert.Null(job3);
        });
    }

    [Fact]
    public Task JobRunner_ParentModel1_ShouldSucceed()
    {
        return SqliteInMemoryHelper.UsingSculleryContextAsync(async context =>
        {
            var jobStore = new EntityFrameworkJobStore(context);
            var jobManager = new JobManager(jobStore);
            var jobRunner = new JobRunner(null!);

            ParentModel model1 = TestJobs.CreateParentModel1();
            await jobManager.EnqueueAsync(() => TestJobs.ParentModelJob1(model1));

            JobDescriptor? job1 = await jobStore.NextAsync(CancellationToken.None);
            Assert.NotNull(job1);

            // The job contains the test assertions.
            await jobRunner.RunAsync(job1!.Call, CancellationToken.None);
        });
    }
}
