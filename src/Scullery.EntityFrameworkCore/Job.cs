using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Scullery.EntityFrameworkCore
{
    public class Job
    {
        public long Id { get; set; }

        public string Name { get; set; }
        // public string Cron { get; set; }
        public DateTime? Scheduled { get; set; }
        public string TimeZone { get; set; }
        public JobStatus Status { get; set; }

        public string Type { get; set; }
        public string Method { get; set; }
        public string Returns { get; set; }
        public bool IsStatic { get; set; }
        public string Parameters { get; set; }
        public string Arguments { get; set; }
    }
}
