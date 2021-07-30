using OnceMi.Framework.Util.Date;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model
{
    public class ResultObject<T> where T : class, new()
    {
        public ResultObject()
        {

        }

        public ResultObject(int code)
        {
            this.Code = code;
            if(this.Code == 0)
            {
                this.Message = "Success";
            }
        }

        public ResultObject(int code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        /// <summary>
        /// 错误编号
        /// </summary>
        public int Code { get; set; } = 0;

        /// <summary>
        /// 错误消息 自定义
        /// </summary>
        public string Message { get; set; } = "Success";

        /// <summary>
        /// JSON创建时间/请求时间
        /// </summary>
        public long Time { get; } = TimeUtil.Timestamp();

        /// <summary>
        /// 自定义数据 需要重载Data
        /// </summary>
        public T Data { get; set; }


    }
}
