using System;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    public class JobOperationRequest : IRequest
    {
        [Required(ErrorMessage = "Id不能为空")]
        [Range(typeof(long), "1", "9223372036854775807", ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
    }
}
