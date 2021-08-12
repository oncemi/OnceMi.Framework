using OnceMi.Framework.Entity.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class IJobPageRequest : IPageRequest
    {
        public JobStatus? Status { get; set; }

        public DateTime? CreateTime { get; set; }

        public string RequestMethod { get; set; }
    }
}
