using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnceMi.Framework.Model.Dto
{
    [MapperFrom(typeof(Organize))]
    [MapperTo(typeof(ICascaderResponse))]
    public class OrganizeItemResponse : ITreeResponse<OrganizeItemResponse>
    {
        /// <summary>
        /// 组织机构名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 组织机构编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 组织类型
        /// </summary>
        public OrganizeType OrganizeType { get; set; }

        /// <summary>
        /// 组织类型名称
        /// </summary>
        public string OrganizeTypeName
        {
            get
            {
                if (this.OrganizeType == 0) return null;
                return this.OrganizeType.GetDescription();
            }
        }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 部门领导名字
        /// </summary>
        public string DepartLeaderNames
        {
            get
            {
                var names = this.DepartLeaders?.Select(p => p.User?.NickName)?.Where(p => !string.IsNullOrEmpty(p))?.ToList();
                if (names == null || names.Count == 0) return null;
                return string.Join(",", names);
            }
        }

        /// <summary>
        /// 部门领导
        /// </summary>
        public List<OrganizeManager> DepartLeaders { get; set; }

        /// <summary>
        /// 分管领导名字
        /// 逗号分隔
        /// </summary>
        public string HeadLeaderNames
        {
            get
            {
                var names = this.HeadLeaders?.Select(p => p.User?.NickName)?.Where(p => !string.IsNullOrEmpty(p))?.ToList();
                if (names == null || names.Count == 0) return null;
                return string.Join(",", names);
            }
        }

        /// <summary>
        /// 分管领导
        /// </summary>
        public List<OrganizeManager> HeadLeaders { get; set; }

        /// <summary>
        /// 树标签
        /// </summary>
        public override string Label
        {
            get
            {
                return this.Name;
            }
            set
            {

            }
        }

        /// <summary>
        /// 创建者Id
        /// </summary>
        public long? CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// 修改者Id
        /// </summary>
        public long? UpdatedUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdatedTime { get; set; }
    }
}
