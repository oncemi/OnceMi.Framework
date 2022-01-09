using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 菜单表
    /// </summary>
    [Table(Name = nameof(Menus))]
    public class Menus : IBaseEntity
    {
        [Column(IsNullable = true)]
        public long? ParentId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column(StringLength = 100, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Column(StringLength = 1000, IsNullable = true)]
        public string Icon { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public MenuType Type { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否隐藏
        /// </summary>
        [Column(IsNullable = false)]
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(IsNullable = false)]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 1000, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// 可关闭
        /// </summary>
        public bool? Closable { get; set; } = true;

        /// <summary>
        /// 打开组
        /// </summary>
        public bool? Opened { get; set; }

        /// <summary>
        /// 打开新窗口
        /// </summary>
        public bool? IsOpenInNewWindow { get; set; } = false;

        /// <summary>
        /// 视图Id
        /// </summary>
        [Column(IsNullable = true)]
        public long? ViewId { get; set; }

        [Navigate(nameof(ViewId))]
        public Views View { get; set; }

        /// <summary>
        /// ApiId
        /// </summary>
        [Column(IsNullable = true)]
        public long? ApiId { get; set; }

        [Navigate(nameof(ApiId))]
        public Apis Api { get; set; }

        /// <summary>
        /// 子条目
        /// </summary>
        [Column(IsIgnore = true)]
        public List<Menus> Children { get; set; }
    }

    public enum MenuType
    {
        [Description("视图")]
        View = 1,

        [Description("接口")]
        Api = 2,

        [Description("分组")]
        Group = 3,
    }
}
