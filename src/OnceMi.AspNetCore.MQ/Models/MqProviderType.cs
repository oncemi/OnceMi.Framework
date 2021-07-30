using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    public enum MqProviderType
    {
        /// <summary>
        /// RabbitMQ
        /// </summary>
        RabbitMQ = 1,

        /// <summary>
        /// Redis
        /// </summary>
        Redis = 2
    }
}
