using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Scullery.EntityFrameworkCore;

namespace Scullery
{
    public class SculleryEntityFrameworkOptions
    {
        public int SleepMilliseconds { get; set; }
    }
}
