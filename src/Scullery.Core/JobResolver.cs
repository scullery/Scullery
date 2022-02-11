using System.Linq.Expressions;

namespace Scullery
{
    public class JobResolver
    {
        public static JobCall Describe(Expression<Action> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null)
                throw new ArgumentException("Expression is not a method", nameof(expression));

            if (member.Method.ReturnType.FullName != "System.Void")
                throw new ArgumentException("Method must return void", nameof(expression));

            return CreateCall(member);
        }

        public static JobCall Describe(Expression<Func<Task>> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null)
                throw new ArgumentException("Expression is not a method", nameof(expression));

            if (member.Method.ReturnType.FullName != "System.Threading.Tasks.Task")
                throw new ArgumentException("Async method must return a Task", nameof(expression));

            return CreateCall(member);
        }

        public static JobCall Describe<T>(Expression<Action<T>> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null)
                throw new ArgumentException("Expression is not a method", nameof(expression));

            if (member.Method.ReturnType.FullName != "System.Void")
                throw new ArgumentException("Method must return void", nameof(expression));

            return CreateCall(member);
        }

        public static JobCall Describe<T>(Expression<Func<T, Task>> expression)
        {
            var member = expression.Body as MethodCallExpression;
            if (member == null)
                throw new ArgumentException("Expression is not a method", nameof(expression));

            if (member.Method.ReturnType.FullName != "System.Threading.Tasks.Task")
                throw new ArgumentException("Async method must return a Task", nameof(expression));

            return CreateCall(member);
        }

        private static JobCall CreateCall(MethodCallExpression member)
        {
            // var args = new List<JobArgument>();
            var args = new List<object>();
            foreach (Expression arg in member.Arguments)
            {
                if (arg is ConstantExpression cexp)
                {
                    args.Add(cexp.Value);
                }
                else if (arg is MemberExpression mexp)
                {
                    args.Add(GetValue(mexp));
                }
                else
                {
                    throw new Exception("Unsupported argument");
                }
            }

            return new JobCall
            {
                Type = member.Method.DeclaringType.AssemblyQualifiedName,
                Method = member.Method.Name,
                Returns = member.Method.ReturnType.FullName,
                IsStatic = member.Object == null,
                Arguments = args.ToArray()
            };
        }

        // https://stackoverflow.com/a/2616980/51558
        private static object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}
