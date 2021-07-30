using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Config
{
    public enum RedisSchema
    {
        Default = 0,
        MasterSlave = 1,
        Sentinel = 2,
        Cluster = 3
    }

    public class RedisSettingNode
    {
        public RedisSchema RedisSchema { get; set; }

        public string SentinelConnectionString { get; set; }

        public List<string> RedisConnectionStrings { get; set; }
    }
}
