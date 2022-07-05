using System;

namespace OnceMi.Framework.Model.Exceptions
{
    public class JobExcuteException : Exception
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
