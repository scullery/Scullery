using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scullery.Test
{
    public class TestJobs
    {
        public static void Job1(int n)
        {
        }

        public static Task AsyncJob1(int n)
        {
            return Task.CompletedTask;
        }

        public static void ErrorJob()
        {
            throw new Exception("Job error");
        }

        public static Task ErrorJobAsync()
        {
            throw new Exception("Async job error");
        }
    }
}
