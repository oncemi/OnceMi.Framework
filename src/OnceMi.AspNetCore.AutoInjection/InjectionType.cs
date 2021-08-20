namespace OnceMi.AspNetCore.AutoInjection
{
    /// <summary>
    /// 注入类型
    /// </summary>
    public enum InjectionType
    {
        /// <summary>
        /// Transient
        /// </summary>
        Transient,

        /// <summary>
        /// Scoped
        /// </summary>
        Scoped,

        /// <summary>
        /// Singleton
        /// </summary>
        Singleton
    }
}