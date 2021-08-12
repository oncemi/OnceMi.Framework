using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class JobOperationRequest : IRequest
    {
        [Required(ErrorMessage = "Id不能为空")]
        [Range(typeof(long), "1", "9223372036854775807", ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
    }
}
