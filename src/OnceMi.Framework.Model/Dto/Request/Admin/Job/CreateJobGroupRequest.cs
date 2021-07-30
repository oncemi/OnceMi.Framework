using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(JobGroups))]
    public class CreateJobGroupRequest : IRequest
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "分组名称不能为空")]
        public string Name { get; set; }


        /// <summary>
        /// 编码
        /// </summary>
        [Required(ErrorMessage = "分组编码不能为空")]
        public string Code { get; set; }
    }
}
