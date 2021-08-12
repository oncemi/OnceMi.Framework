using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class JobHistoryPageRequest : IPageRequest
    {
        [Required(ErrorMessage = "作业Id不能为空")]
        [Range(100000, long.MaxValue, ErrorMessage = "作业Id不能为空")]
        public long? JobId { get; set; }
    }
}
