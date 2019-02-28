using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;
using Scullery;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JobServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Scullery services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        public static IServiceCollection AddScullery(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IHostedService, JobService>();
            services.AddScoped<JobManager>();

            return services;
        }

        /// <summary>
        /// Adds Scullery services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action"/> to configure the provided <see cref="SculleryOptions"/>.</param>
        public static IServiceCollection AddScullery(this IServiceCollection services, Action<ScullerySetupOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            var options = new ScullerySetupOptions
            {
                InMemoryStore = false
            };

            setupAction?.Invoke(options);

            if (options.InMemoryStore)
            {
                services.AddSingleton<IJobStore, MemoryJobStore>();
            }

            return services.AddScullery();
        }
    }
}
