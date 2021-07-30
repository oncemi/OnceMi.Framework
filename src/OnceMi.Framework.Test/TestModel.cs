using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Test
{
    class TestModel
    {
        public string UserName { get; set; } = "张三";

        public int Age { get; set; } = 20;

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime? UpdateTime { get; set; }
    }
}
