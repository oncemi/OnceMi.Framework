using OnceMi.Framework.Model.Attributes;
using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class CreateOrganizeRequest : IRequest
    {
        private long? _parentId;

        /// <summary>
        /// 父Id
        /// </summary>
        public long? ParentId
        {
            get
            {
                if (_parentId == 0) return null;
                return _parentId;
            }
            set
            {
                _parentId = value;
            }
        }

        /// <summary>
        /// 组织机构名称
        /// </summary>
        [Required(ErrorMessage = "组织名称不能为空")]
        public string Name { get; set; }

        /// <summary>
        /// 组织机构编码
        /// </summary>
        [Required(ErrorMessage = "组织代码不能为空")]
        public string Code { get; set; }

        /// <summary>
        /// 组织类型
        /// </summary>
        [Required(ErrorMessage = "组织类型不能为空")]
        public OrganizeType OrganizeType { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 部门领导
        /// </summary>
        public List<long> DepartLeaders { get; set; }

        /// <summary>
        /// 分管领导
        /// </summary>
        public List<long> HeadLeaders { get; set; }
    }
}
