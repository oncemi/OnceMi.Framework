using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class DeleteCachesResponse : IResponse
    {
        public DeleteCachesResponse(long count)
        {
            this.Count = count;
        }

        public long Count { get; set; }
    }
}
