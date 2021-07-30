using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class QueryApiPageRequest : IPageRequest
    {
        public string ApiVersion { get; set; }
    }
}
