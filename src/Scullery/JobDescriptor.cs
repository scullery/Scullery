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

    public class JobCall
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public string Returns { get; set; }
        public bool IsStatic { get; set; }
        public object[] Arguments { get; set; }
    }
}
