using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Scullery
{
    public class JobManager
    {
        private readonly IJobStore _jobStore;

        public JobManager(IJobStore jobStore)
        {
            _jobStore = jobStore;
        }

        public Task<string> EnqueueAsync(Expression<Action> expression)
        {
            JobDescriptor job = JobResolver.Describe(expression);
            return _jobStore.EnqueueAsync(job);
        }

        public Task<string> EnqueueAsync(Expression<Func<Task>> expression)
        {
            JobDescriptor job = JobResolver.Describe(expression);
            return _jobStore.EnqueueAsync(job);
        }
    }
}
