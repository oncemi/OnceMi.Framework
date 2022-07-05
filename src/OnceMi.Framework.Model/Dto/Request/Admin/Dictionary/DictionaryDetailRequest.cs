namespace OnceMi.Framework.Model.Dto
{
    public class DictionaryDetailRequest : IRequest
    {
        /// <summary>
        /// 使用Id查询
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 使用Code查询
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 是否包含子节点
        /// </summary>
        public bool IncludeChild { get; set; } = false;
    }
}
