namespace Scullery
{
    public interface IJobRunner
    {
        Task RunAsync(JobCall job, CancellationToken cancellationToken);
    }

    public class JobRunner : IJobRunner
    {
        public const string VoidTypeName = "System.Void";
        public const string TaskTypeName = "System.Threading.Tasks.Task";

        private readonly IServiceProvider _services;

        public JobRunner(IServiceProvider services)
        {
            _services = services;
        }

        public async Task RunAsync(JobCall job, CancellationToken cancellationToken)
        {
            if (job.Returns == TaskTypeName)
            {
                await InvokeAsync(job, cancellationToken);
            }
            else if (job.Returns == VoidTypeName)
            {
                Invoke(job, cancellationToken);
            }
            else
            {
                throw new Exception($"Unsupported job return type '{job.Returns}'");
            }
        }

        public void Invoke(JobCall job, CancellationToken cancellationToken)
        {
            if (job.Returns != VoidTypeName)
                throw new ArgumentException("Method must return void", nameof(job));

            InvokeMember(job.Type, job.IsStatic, job.Method, job.Arguments);
        }

        public Task InvokeAsync(JobCall job, CancellationToken cancellationToken)
        {
            if (job.Returns != TaskTypeName)
                throw new ArgumentException("Async method must return a Task", nameof(job));

            object result = InvokeMember(job.Type, job.IsStatic, job.Method, job.Arguments);

            return (Task)result;
        }

        public object InvokeMember(string assemblyQualifiedName, bool isStatic, string methodName, object[] args)
        {
            Type calledType = Type.GetType(assemblyQualifiedName);
            object instance = null;
            BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Public;
            if (isStatic)
            {
                flags |= BindingFlags.Static;
            }
            else
            {
                flags |= BindingFlags.Instance;
                // instance = Activator.CreateInstance(calledType);
                instance = ActivatorUtilities.CreateInstance(_services, calledType);
            }
            return calledType.InvokeMember(methodName, flags, null, instance, args);
        }
    }
}
