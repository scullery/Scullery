namespace Scullery;

public class ScullerySetupOptions
{
    /// <summary>
    /// Use simple in-memory store for getting started and experimenting.
    /// </summary>
    /// <remarks>This store is not production ready and can only be used with a single instance.</remarks>
    public bool InMemoryStore { get; set; }
}

public class SculleryOptions
{
    /// <summary>
    /// An optional period after which any job will be cancelled.
    /// </summary>
    public TimeSpan? JobTimeout { get; set; }
}
