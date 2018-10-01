using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Scullery
{
    public class JobResolver
    {
        public static JobDescriptor Describe(Expression<Action> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null)
                throw new ArgumentException("Expression is not a method", nameof(expression));

            if (member.Method.ReturnType.FullName != "System.Void")
                throw new ArgumentException("Method must return void", nameof(expression));

            return CreateDescriptor(member);
        }

        public static JobDescriptor Describe(Expression<Func<Task>> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null)
                throw new ArgumentException("Expression is not a method", nameof(expression));

            if (member.Method.ReturnType.FullName != "System.Threading.Tasks.Task")
                throw new ArgumentException("Async method must return a Task", nameof(expression));

            return CreateDescriptor(member);
        }

        public static JobDescriptor Describe<T>(Expression<Action<T>> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null)
                throw new ArgumentException("Expression is not a method", nameof(expression));

            if (member.Method.ReturnType.FullName != "System.Void")
                throw new ArgumentException("Method must return void", nameof(expression));

            return CreateDescriptor(member);
        }

        public static JobDescriptor Describe<T>(Expression<Func<T, Task>> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null)
                throw new ArgumentException("Expression is not a method", nameof(expression));

            if (member.Method.ReturnType.FullName != "System.Threading.Tasks.Task")
                throw new ArgumentException("Async method must return a Task", nameof(expression));

            return CreateDescriptor(member);
        }

        private static JobDescriptor CreateDescriptor(MethodCallExpression member)
        {
            var args = new List<JobArgument>();
            foreach (Expression arg in member.Arguments)
            {
                if (arg is ConstantExpression)
                {
                    var exp = arg as ConstantExpression;
                    args.Add(new JobArgument { Type = exp.Type.FullName, Value = exp.Value });
                }
                else
                {
                    throw new Exception("Unsupported argument");
                }
            }

            return new JobDescriptor
            {
                Type = member.Method.DeclaringType.AssemblyQualifiedName,
                Method = member.Method.Name,
                Returns = member.Method.ReturnType.FullName,
                IsStatic = member.Object == null,
                Arguments = args
            };
        }
    }
}
