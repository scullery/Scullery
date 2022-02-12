using Scullery.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Builder;

public static class SculleryEntityFrameworkApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Scullery Entity Framework Core services to <see cref="IApplicationBuilder"/>.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/></param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IApplicationBuilder UseSculleryEntityFramework(this IApplicationBuilder app)
    {
        app.ApplicationServices.InitializeDatabase<SculleryContext>();
        return app;
    }
}
