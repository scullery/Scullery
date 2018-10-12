using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Scullery.EntityFrameworkCore.Test
{
    public class SqliteInMemoryHelper
    {
        public static async Task UsingSculleryContextAsync(Func<SculleryContext, Task> callback, Func<SculleryContext, Task> second = null)
        {
            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<SculleryContext>()
                    .UseSqlite(connection)
                    .Options;

                // Create the schema in the database
                using (var context = new SculleryContext(options))
                {
                    context.Database.EnsureCreated();
                }

                // Run the test against one instance of the context
                using (var context = new SculleryContext(options))
                {
                    await callback(context);
                }

                if (second != null)
                {
                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new SculleryContext(options))
                    {
                        await second(context);
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
