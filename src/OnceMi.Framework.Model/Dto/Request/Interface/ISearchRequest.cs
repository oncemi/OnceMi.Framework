
namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 关键字查询请求
    /// </summary>
    public class ISearchRequest : IRequest
    {
        /// <summary>
        /// 查询关键字
        /// </summary>
        public string Search { get; set; }
    }
}
