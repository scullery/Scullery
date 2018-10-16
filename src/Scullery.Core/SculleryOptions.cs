using System;
using System.Collections.Generic;
using System.Text;

namespace Scullery
{
    public class SculleryOptions
    {
        /// <summary>
        /// Use simple in-memory store for getting started and experimenting.
        /// </summary>
        /// <remarks>This store is not production ready and can only be used with a single instance.</remarks>
        public bool InMemoryStore { get; set; }
    }
}
