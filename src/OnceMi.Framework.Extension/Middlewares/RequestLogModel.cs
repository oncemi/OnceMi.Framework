using Microsoft.AspNetCore.Http;
using OnceMi.Framework.Util.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Middlewares
{
    class RequestLogModel
    {
        public string Url { get; set; }

        public IHeaderDictionary Headers { get; set; }

        public string Method { get; set; }

        public string RequestBody { get; set; }

        public string ResponseBody { get; set; }

        public long Elapsed { get; set; }

        public int StatusCode { get; set; }

        public override string ToString()
        {
            return $"\r\n         Url: {this.Url}\r\n     Headers: {JsonUtil.SerializeToString(Headers)}\r\n      Method: {this.Method}\r\n  StatusCode: {this.StatusCode}\r\n RequestBody: {this.RequestBody}\r\nResponseBody: {this.ResponseBody}\r\n     Elapsed: {this.Elapsed}ms";
        }
    }
}
