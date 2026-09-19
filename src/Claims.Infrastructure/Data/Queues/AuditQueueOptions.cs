using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Infrastructure.Data.Queues
{
    public sealed class AuditQueueOptions
    {
        public const string SectionName = "AuditQueue";

        public int Capacity { get; init; } = 1_000;
    }
}
