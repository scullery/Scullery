﻿namespace Scullery.Test;

public class TestJobs
{
    public static void Job1(int n)
    {
    }

    public static Task AsyncJob1(int n)
    {
        return Task.CompletedTask;
    }

    public void InstanceJob1(int n)
    {
    }

    public Task InstanceJobAsync1(int n)
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
