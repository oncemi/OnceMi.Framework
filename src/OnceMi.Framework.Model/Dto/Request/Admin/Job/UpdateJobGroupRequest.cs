using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(JobGroup))]
    public class UpdateJobGroupRequest : IUpdateRequest
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
