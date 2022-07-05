namespace OnceMi.Framework.Model.Dto
{
    public class ISelectResponse<TValue>
    {
        /// <summary>
        /// 值
        /// </summary>
        public virtual TValue Value { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public virtual string Name { get; set; }
    }
}
