using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scullery.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SculleryServiceProviderExtensions
    {
        public static void InitializeSculleryDatabase(this IServiceProvider serviceProvider)
        {
            serviceProvider.InitializeDatabase<SculleryContext>();
        }
    }
}
