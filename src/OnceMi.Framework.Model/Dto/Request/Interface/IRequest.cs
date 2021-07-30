using AutoMapper;
using OnceMi.Framework.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public abstract class IRequest
    {
        /// <summary>
        /// 为当前Request创建Map到TResult的方法
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual TResult MapTo<TResult>(TResult result) where TResult : class, new()
        {
            var config = new MapperConfiguration(cfg =>
            {
                var finalType = this.GetType().UnderlyingSystemType;
                cfg.CreateMap(finalType, typeof(TResult)).IgnoreDifferentTypeProperty(finalType, typeof(TResult));
            });
            var mapper = config.CreateMapper();
            return mapper.Map(this, result);
        }
    }
}
