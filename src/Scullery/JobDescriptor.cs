using System;
using System.Collections.Generic;
using System.Text;

namespace Scullery
{
    public class JobArgument
    {
        public string Type { get; set; }
        public object Value { get; set; }
    }

    public class JobDescriptor
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public string Returns { get; set; }
        public List<JobArgument> Arguments { get; set; }
    }
}
