using System;

namespace OnceMi.Framework.Config
{
    public class HealthCheckNode
    {
        /// <summary>
        /// 是否开启HealthCheckUI
        /// </summary>
        public bool IsEnabledHealthCheckUI { get; set; }

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
