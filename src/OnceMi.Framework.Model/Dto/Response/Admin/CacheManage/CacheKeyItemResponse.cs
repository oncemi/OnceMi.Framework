namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 缓存Key
    /// </summary>
    public class CacheKeyItemResponse : IResponse
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}
