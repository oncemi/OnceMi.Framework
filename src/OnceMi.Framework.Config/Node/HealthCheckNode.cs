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

        private string _healthCheckUIPath = null;

        public string HealthCheckUIPath
        {
            get
            {
                return _healthCheckUIPath;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception("Health check ui path can not null. Please check your app setting.");
                }
                if (!value.StartsWith('/'))
                {
                    throw new Exception("Health check ui path only support relative path ,look like '/sys/health'");
                }
                _healthCheckUIPath = value;
            }
        }

        private string _healthCheckEndpoint = null;

        public string HealthCheckEndpoint
        {
            get
            {
                return _healthCheckEndpoint;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception("Health check endpoint can not null. Please check your app setting.");
                }
                if (!value.StartsWith('/'))
                {
                    throw new Exception("Health check endpoint only support relative path ,look like '/sys/health-ui'");
                }
                _healthCheckEndpoint = value;
            }
        }

        public int EvaluationTimeinSeconds { get; set; }

        public int MinimumSecondsBetweenFailureNotifications { get; set; } = 60;

        public int MaximumHistoryEntriesPerEndpoint { get; set; }
    }
}
