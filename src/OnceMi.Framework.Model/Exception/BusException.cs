using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Exception
{
    public class BusException : System.Exception
    {
        public int Code { get; set; }

        public BusException(int code)
        {
            this.Code = code;
        }

        public BusException(int code, string message) : base(message)
        {
            this.Code = code;
        }

        public BusException(int code, string message, System.Exception ex) : base(message, ex)
        {
            this.Code = code;
        }
    }
}
