using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 返回树形对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ITreeResponse<T> : IResponse where T : class
    {
        public virtual long Id { get; set; }

        public virtual long? ParentId { get; set; }

        public abstract string Label { get; set; }

        public virtual ScopedSlots ScopedSlots { get; set; } = new ScopedSlots(nameof(Label).ToLower());

        public virtual List<T> Children { get; set; }
    }

    public class ScopedSlots
    {
        public ScopedSlots()
        {

        }

        public ScopedSlots(string title)
        {
            this.Title = title;
        }

        public string Title { get; set; } = "label";

        public string Icon { get; set; } = "";
    }
}
