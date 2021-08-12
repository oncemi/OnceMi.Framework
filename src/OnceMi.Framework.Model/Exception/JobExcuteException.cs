using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Exception
{
    public class JobExcuteException : System.Exception
    {
        public JobExcuteException()
        {

        }

        public JobExcuteException(string message) : base(message)
        {

        }

        public JobExcuteException(string message, System.Exception innerException) : base(message, innerException)
        {

        }

        public object Result { get; set; }
    }
}
