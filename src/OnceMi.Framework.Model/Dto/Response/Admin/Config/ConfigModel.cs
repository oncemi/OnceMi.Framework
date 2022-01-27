using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;

namespace OnceMi.Framework.Model.Dto.Response.Admin.Config
{
    [MapperFrom(typeof(Entity.Admin.Config))]
    public class ConfigModel<T> where T : class, new()
    {
        public long Id { get; set; }

        /// <summary>
        /// 键名
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 配置内容
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

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
        public long UpdatedUserId { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public virtual DateTime? UpdatedTime { get; set; }
    }
}
