using System;

namespace OnceMi.AspNetCore.AutoInjection
{
    /// <summary>
    /// 服务注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AutoInjectionAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Type Interface { get; private set; }

        /// <summary>
        /// 注入类型
        /// </summary>
        public InjectionType InjectionType { get; private set; }

        /// <summary>
        /// 服务注入
        /// </summary>
        public AutoInjectionAttribute()
        {
            this.Interface = null;
            this.InjectionType = InjectionType.Scoped;
        }

        /// <summary>
        /// 服务注入
        /// </summary>
        /// <param name="injectionType">注入类型</param>
        public AutoInjectionAttribute(InjectionType injectionType)
        {
            this.Interface = null;
            this.InjectionType = injectionType;
        }

        /// <summary>
        /// 服务注入
        /// </summary>
        /// <param name="interfaceType">服务的接口类型</param>
        public AutoInjectionAttribute(Type interfaceType)
        {
            this.Interface = interfaceType;
            this.InjectionType = InjectionType.Scoped;
        }

        /// <summary>
        /// 服务注入
        /// </summary>
        /// <param name="interfaceType">服务的接口类型</param>
        /// <param name="injectionType">注入的类型</param>
        public AutoInjectionAttribute(Type interfaceType, InjectionType injectionType)
        {
            this.Interface = interfaceType;
            this.InjectionType = injectionType;
        }
    }
}
