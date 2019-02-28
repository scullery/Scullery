using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scullery
{
    public interface IJobServiceEvents
    {
        Task OnStoppedAsync(string message, Exception ex);
    }
}
