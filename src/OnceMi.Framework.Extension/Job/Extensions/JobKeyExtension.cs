using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public static class JobKeyExtension
    {
        public static long GetId(this JobKey jobKey)
        {
            if (jobKey == null || string.IsNullOrEmpty(jobKey.Name))
            {
                throw new Exception("JobKey is null or JobKey name is null");
            }
            if (long.TryParse(jobKey.Name, out long id))
            {
                return id;
            }
            else
            {
                throw new Exception($"Parse job key name '{jobKey.Name}' to long id failed.");
            }
        }
    }
}
