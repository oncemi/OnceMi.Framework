using System;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    public class JobHistoryPageRequest : IPageRequest
    {
        [Required(ErrorMessage = "作业Id不能为空")]
        [Range(100000, long.MaxValue, ErrorMessage = "作业Id不能为空")]
        public long? JobId { get; set; }
    }
}
