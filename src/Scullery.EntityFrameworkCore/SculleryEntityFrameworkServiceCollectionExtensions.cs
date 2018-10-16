using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Scullery;
using Scullery.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SculleryEntityFrameworkServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Scullery Entity Framework Core services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        public static IServiceCollection AddSculleryEntityFramework(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<IJobStore, EntityFrameworkJobStore>();

            return services;
        }

        /// <summary>
        /// Adds Scullery Entity Framework Core sservices to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action"/> to configure the provided <see cref="DbContextOptionsBuilder"/>.</param>
        public static IServiceCollection AddSculleryEntityFramework(this IServiceCollection services, Action<DbContextOptionsBuilder> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddDbContext<SculleryContext>(dbOptions =>
            {
                setupAction?.Invoke(dbOptions);
            }, ServiceLifetime.Scoped);

            return services.AddSculleryEntityFramework();
        }

        /// <summary>
        /// Adds Scullery Entity Framework Core sservices to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action"/> to configure the provided <see cref="DbContextOptionsBuilder"/> and <see cref="SculleryEntityFrameworkOptions"/>.</param>
        public static IServiceCollection AddSculleryEntityFramework(this IServiceCollection services, Action<DbContextOptionsBuilder, SculleryEntityFrameworkOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            var options = new SculleryEntityFrameworkOptions
            {
            };

            services.AddDbContext<SculleryContext>(dbOptions =>
            {
                setupAction?.Invoke(dbOptions, options);
            }, ServiceLifetime.Scoped);

            return services.AddSculleryEntityFramework();
        }
    }
}
