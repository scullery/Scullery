namespace Scullery.Test;

public class JobManagerTest
{
    [Fact]
    public async Task Enqueue_VoidMethod_ShouldSucceed()
    {
        var testStore = new MemoryJobStore();
        var jobManager = new JobManager(testStore);
        var jobRunner = new JobRunner(null);

        await jobManager.EnqueueAsync(() => TestJobs.Job1(1));
        JobDescriptor job = await testStore.NextAsync(CancellationToken.None);
        Assert.Equal("Job1", job.Call.Method);
        jobRunner.Invoke(job.Call, CancellationToken.None);
    }

    [Fact]
    public async Task Enqueue_AsyncMethod_ShouldSucceed()
    {
        var testStore = new MemoryJobStore();
        var jobManager = new JobManager(testStore);
        var jobRunner = new JobRunner(null);

        await jobManager.EnqueueAsync(() => TestJobs.AsyncJob1(1));
        JobDescriptor job = await testStore.NextAsync(CancellationToken.None);
        Assert.Equal("AsyncJob1", job.Call.Method);
        await jobRunner.InvokeAsync(job.Call, CancellationToken.None);
    }

    [Fact]
    public async Task Enqueue_VoidInstanceMethod_ShouldSucceed()
    {
        var testServiceProvider = new TestServiceProvider();
        var testStore = new MemoryJobStore();
        var jobManager = new JobManager(testStore);
        var jobRunner = new JobRunner(testServiceProvider);

        await jobManager.EnqueueAsync<TestJobs>((t) => t.InstanceJob1(1));
        JobDescriptor job = await testStore.NextAsync(CancellationToken.None);
        Assert.Equal("InstanceJob1", job.Call.Method);
        jobRunner.Invoke(job.Call, CancellationToken.None);
    }

    [Fact]
    public async Task Enqueue_AsyncInstanceMethod_ShouldSucceed()
    {
        var testServiceProvider = new TestServiceProvider();
        var testStore = new MemoryJobStore();
        var jobManager = new JobManager(testStore);
        var jobRunner = new JobRunner(testServiceProvider);

        await jobManager.EnqueueAsync<TestJobs>((t) => t.InstanceJobAsync1(1));
        JobDescriptor job = await testStore.NextAsync(CancellationToken.None);
        Assert.Equal("InstanceJobAsync1", job.Call.Method);
        await jobRunner.InvokeAsync(job.Call, CancellationToken.None);
    }
}
