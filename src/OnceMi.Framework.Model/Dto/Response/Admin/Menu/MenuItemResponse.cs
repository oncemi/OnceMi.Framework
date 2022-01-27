using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Extensions;
using OnceMi.Framework.Util.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 菜单DTO
    /// </summary>
    [MapperFrom(typeof(Menu))]
    [MapperTo(typeof(ICascaderResponse))]
    public class MenuItemResponse : ITreeResponse<MenuItemResponse>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 键名称
        /// </summary>
        public string Code
        {
            get
            {
                if(this.Type == MenuType.Api && this.Api != null)
                {
                    return this.Api.Code;
                }
                return null;
            }
        }

        private string _path;

        /// <summary>
        /// 路径
        /// </summary>
        public string Path
        {
            get
            {
                if (!string.IsNullOrEmpty(_path))
                {
                    return _path;
                }
                if (Type == MenuType.Api && ApiId != null && Api != null)
                {
                    return Api.Path;
                }
                if(Type == MenuType.View && ViewId != null && View != null)
                {
                    if (!string.IsNullOrEmpty(View.Path))
                    {
                        return View.Path;
                    }
                    return View.Router;
                }
                return _path;
            }
            set
            {
                _path = value;
            }
        }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public MenuType Type { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName
        {
            get
            {
                return this.Type.GetDescription();
            }
        }

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

        public bool IsLink
        {
            get
            {
                return !string.IsNullOrEmpty(_path);
            }
        }

        /// <summary>
        /// 创建者Id
        /// </summary>
        public long CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// 视图Id
        /// </summary>
        public long? ViewId { get; set; }

        public ViewItemResponse View { get; set; }

        /// <summary>
        /// ApiId
        /// </summary>
        public long? ApiId { get; set; }

        public ApiItemResponse Api { get; set; }

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
    }
}
