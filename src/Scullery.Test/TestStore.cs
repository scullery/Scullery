using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Scullery.Test
{
    public class TestStore : IJobStore
    {
        private int _idSource = 0;
        private List<(int jobId, string jobJson)> _jobs = new List<(int, string)>();

        public Task<JobRecord> NextAsync(CancellationToken cancellationToken)
        {
            (int jobId, string json) = _jobs.First();
            _jobs.RemoveAt(0);
            JobDescriptor descriptor = JsonConvert.DeserializeObject<JobDescriptor>(json);
            return Task.FromResult(new JobRecord
            {
                Id = jobId.ToString(),
                Descriptor = descriptor
            });
        }

        public Task<string> EnqueueAsync(JobDescriptor job)
        {
            string json = JsonConvert.SerializeObject(job);
            int jobId = ++_idSource;
            _jobs.Add((jobId, json));

            return Task.FromResult(jobId.ToString());
        }

        public Task SucceededAsync(string id)
        {
            return Task.CompletedTask;
        }

        public Task FailedAsync(string id, Exception ex)
        {
            return Task.CompletedTask;
        }
    }
}
