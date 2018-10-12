using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Scullery.EntityFrameworkCore
{
    public interface IEntityFrameworkJobStoreQueue
    {
        Task<Job> TryNextAsync();
    }

    public class EntityFrameworkJobStoreOptions
    {
        public int SleepMilliseconds { get; set; }
    }

    public class EntityFrameworkJobStore : IJobStore
    {
        public readonly SculleryContext _context;
        public readonly IEntityFrameworkJobStoreQueue _queue;

        int _sleepMilliseconds;

        const int _defaultSleepMilliseconds = 15 * 1000;

        public EntityFrameworkJobStore(
            SculleryContext context,
            IOptions<EntityFrameworkJobStoreOptions> options = null,
            IEntityFrameworkJobStoreQueue queue = null)
        {
            _context = context;
            _queue = queue ?? new SqlJobStoreQueue(context);

            if (options?.Value == null)
            {
                _sleepMilliseconds = _defaultSleepMilliseconds;
            }
            else
            {
                _sleepMilliseconds = options.Value.SleepMilliseconds <= 0 
                    ? _defaultSleepMilliseconds
                    : options.Value.SleepMilliseconds;
            }
        }

        /// <summary>
        /// If a job is available, returns it. Otherwise, returns null.
        /// </summary>
        /// <returns>The the descriptor of the next job, if available.</returns>
        public async Task<JobDescriptor> TryNextAsync()
        {
            Job job = await _queue.TryNextAsync();
            if (job == null)
            {
                return null;
            }

            return new JobDescriptor
            {
                Id = job.Id.ToString(),
                Name = job.Name,
                // Cron = job.Cron,
                Scheduled = job.Scheduled,
                TimeZone = job.TimeZone == null 
                    ? null 
                    : TimeZoneInfo.FromSerializedString(job.TimeZone),
                Status = job.Status,
                Call = new JobCall
                {
                    Type = job.Type,
                    Method = job.Method,
                    Returns = job.Returns,
                    IsStatic = job.IsStatic,
                    Arguments = DeserializeArguments(job.Parameters, job.Arguments)
                }
            };
        }

        /// <summary>
        /// Waits until a job is ready and returns it.
        /// </summary>
        /// <returns>The the descriptor of the next job.</returns>
        public async Task<JobDescriptor> NextAsync(CancellationToken cancellationToken)
        {
            JobDescriptor job;

            do
            {
                job = await TryNextAsync();
                if (job == null)
                {
                    await Task.Delay(_defaultSleepMilliseconds, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Returning null is OK in this case.
                        return null;
                    }
                }
            } while (job == null);

            return job;
        }

        public async Task<string> EnqueueAsync(JobCall job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            Job result = await EnqueueAsync(job, JobStatus.Ready, null);

            return result.Id.ToString();
        }

        public async Task<string> ScheduleAsync(JobCall job, DateTime scheduled, TimeZoneInfo timeZone = null)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            Job result = await EnqueueAsync(job, JobStatus.Waiting, scheduled, timeZone);

            return result.Id.ToString();
        }

        public Task RecurrentAsync(string name, string cron, JobCall job, TimeZoneInfo timeZone = null)
        {
            return Task.CompletedTask;
        }

        //Task TriggerAsync(string name, string cron, JobCall job, TimeZoneInfo timeZone = null);
        //Task RemoveRecurrentAsync(string name);
        //Task DeleteJobAsync(string id);
        public Task SucceededAsync(string id)
        {
            return Task.CompletedTask;
        }

        public Task FailedAsync(string id, Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task<int> GetCountByStatusAsync(JobStatus status)
        {
            return Task.FromResult(0);
        }

        private async Task<Job> EnqueueAsync(JobCall call, JobStatus status, DateTime? scheduled, TimeZoneInfo timeZone = null)
        {
            if (call == null)
            {
                throw new ArgumentNullException(nameof(call));
            }

            if (status == JobStatus.Waiting && !scheduled.HasValue)
            {
                throw new ArgumentNullException(nameof(scheduled));
            }

            (string parameters, string arguments) = SerializeArguments(call);

            var job = new Job
            {
                Status = status,
                Scheduled = scheduled,
                TimeZone = timeZone == null ? null : timeZone.ToSerializedString(),

                Type = call.Type,
                Method = call.Method,
                Returns = call.Returns,
                IsStatic = call.IsStatic,
                Parameters = parameters,
                Arguments = arguments,
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return job;
        }

        private (string parameters, string arguments) SerializeArguments(JobCall call)
        {
            string parameters = null;
            string arguments = null;
            if (call.Arguments != null && call.Arguments.Length > 0)
            {
                var parms = new List<string>();
                var args = new List<string>();

                foreach (object arg in call.Arguments)
                {
                    parms.Add(arg.GetType().AssemblyQualifiedName);
                    args.Add(JsonConvert.SerializeObject(arg));
                }

                parameters = JsonConvert.SerializeObject(parms);
                arguments = JsonConvert.SerializeObject(args);
            }

            return (parameters, arguments);
        }

        private object[] DeserializeArguments(string parameters, string arguments)
        {
            List<string> parms = JsonConvert.DeserializeObject<List<string>>(parameters);
            List<string> args = JsonConvert.DeserializeObject<List<string>>(arguments);
            if (parms.Count != args.Count)
            {
                throw new Exception("Parameter and argument count mismatch");
            }

            var list = new List<object>();
            for (int i = 0; i < parms.Count; i++)
            {
                Type type = Type.GetType(parms[i]);
                object value = JsonConvert.DeserializeObject(args[i], type);
                list.Add(value);
            }

            return list.ToArray();
        }
    }
}
