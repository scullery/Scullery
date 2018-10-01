using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Scullery.Test
{
    public class JobManagerTest
    {
        [Fact]
        public async Task Enqueue_VoidMethod_ShouldSucceed()
        {
            var testStore = new TestStore();
            var jobManager = new JobManager(testStore);
            var jobRunner = new JobRunner();

            await jobManager.EnqueueAsync(() => TestJobs.Job1(1));
            JobRecord job = await testStore.NextAsync(CancellationToken.None);
            Assert.Equal("Job1", job.Descriptor.Method);
            jobRunner.Invoke(job.Descriptor, CancellationToken.None);
        }

        [Fact]
        public async Task Enqueue_AsyncMethod_ShouldSucceed()
        {
            var testStore = new TestStore();
            var jobManager = new JobManager(testStore);
            var jobRunner = new JobRunner();

            await jobManager.EnqueueAsync(() => TestJobs.AsyncJob1(1));
            JobRecord job = await testStore.NextAsync(CancellationToken.None);
            Assert.Equal("AsyncJob1", job.Descriptor.Method);
            await jobRunner.InvokeAsync(job.Descriptor, CancellationToken.None);
        }
    }
}
