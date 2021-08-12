using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Config
{
    public class HealthCheckNode
    {
        public string HealthCheckName { get; set; }

        public string HealthCheckUIPath { get; set; }

        public int EvaluationTimeinSeconds { get; set; }

        public int MinimumSecondsBetweenFailureNotifications { get; set; } = 60;

        public int MaximumHistoryEntriesPerEndpoint { get; set; }
    }
}
