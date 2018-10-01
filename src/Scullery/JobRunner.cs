﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Scullery
{
    public interface IJobRunner
    {
        Task RunAsync(JobDescriptor job, CancellationToken cancellationToken);
        void Invoke(JobDescriptor job, CancellationToken cancellationToken);
        Task InvokeAsync(JobDescriptor job, CancellationToken cancellationToken);
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

        public async Task RunAsync(JobDescriptor job, CancellationToken cancellationToken)
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

        public void Invoke(JobDescriptor job, CancellationToken cancellationToken)
        {
            if (job.Returns != VoidTypeName)
                throw new ArgumentException("Method must return void", nameof(job));

            object[] args = CoerceArguments(job.Arguments);

            InvokeMember(job.Type, job.IsStatic, job.Method, args);
        }

        public Task InvokeAsync(JobDescriptor job, CancellationToken cancellationToken)
        {
            if (job.Returns != TaskTypeName)
                throw new ArgumentException("Async method must return a Task", nameof(job));

            object[] args = CoerceArguments(job.Arguments);

            object result = InvokeMember(job.Type, job.IsStatic, job.Method, args);

            return (Task)result;
        }

        private object[] CoerceArguments(List<JobArgument> arguments)
        {
            return arguments.Select(a =>
            {
                Type type = Type.GetType(a.Type);
                return Convert.ChangeType(a.Value, type);
            }).ToArray();
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
