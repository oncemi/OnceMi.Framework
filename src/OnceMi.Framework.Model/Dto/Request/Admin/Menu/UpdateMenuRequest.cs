using OnceMi.Framework.Entity.Admin;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    public class UpdateMenuRequest : IUpdateRequest
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
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "菜单名称不能为空")]
        [MaxLength(10, ErrorMessage = "菜单名称不能超过10个字")]
        public string Name { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        [Required(ErrorMessage = "菜单类型不能为空")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MenuType Type { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 描述
        /// </summary>
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

        private long? _viewId;

        /// <summary>
        /// 视图Id
        /// </summary>
        public long? ViewId
        {
            get
            {
                if (_viewId == 0) return null;
                return _viewId;
            }
            set
            {
                _viewId = value;
            }
        }

        private long? _apiId;

        /// <summary>
        /// ApiId
        /// </summary>
        public long? ApiId
        {
            get
            {
                if (_apiId == 0) return null;
                return _apiId;
            }
            set
            {
                _apiId = value;
            }
        }
    }
}
