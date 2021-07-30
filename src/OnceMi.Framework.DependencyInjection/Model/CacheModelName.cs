using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.DependencyInjection
{
    static class CacheModelName
    {
        public static string InMemoryName { get; set; } = "InMemoryModelForOnceMi";

        public static string RedisName { get; set; } = "RedisModelForOnceMi";
    }
}
