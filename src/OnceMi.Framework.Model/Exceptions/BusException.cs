using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Util.Extensions;
using System;

namespace OnceMi.Framework.Model.Exceptions
{
    public class BusException : Exception
    {
        public ResultCode Code { get; set; }

        public override string Message { get; }

        public BusException(ResultCode code) : base()
        {
            this.Code = code;
            this.Message = code.GetDescription();
        }

        public BusException(ResultCode code, string message) : base(message)
        {
            this.Code = code;
            this.Message = string.IsNullOrWhiteSpace(message) ? code.GetDescription() : message;
        }

        public BusException(ResultCode code, string message, System.Exception ex) : base(message, ex)
        {
            this.Code = code;
            this.Message = string.IsNullOrWhiteSpace(message) ? code.GetDescription() : message;
        }
    }
}
