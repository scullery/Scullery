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
            return _jobStore.EnqueueAsync(JobResolver.Describe(expression));
        }

        public Task<string> EnqueueAsync(Expression<Func<Task>> expression)
        {
            return _jobStore.EnqueueAsync(JobResolver.Describe(expression));
        }

        public Task<string> EnqueueAsync<T>(Expression<Action<T>> expression)
        {
            return _jobStore.EnqueueAsync(JobResolver.Describe<T>(expression));
        }

        public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> expression)
        {
            return _jobStore.EnqueueAsync(JobResolver.Describe<T>(expression));
        }

        public Task RecurrentAsync<T>(string name, string cron, Expression<Func<T, Task>> expression, TimeZoneInfo timeZone = null)
        {
            return _jobStore.RecurrentAsync(name, cron, JobResolver.Describe<T>(expression));
        }
    }
}
