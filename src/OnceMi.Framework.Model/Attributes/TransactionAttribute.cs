using FreeSql;
using System;
using System.Data;

namespace OnceMi.Framework.Model.Attributes
{
    /// <summary>
    /// 数据库数据
    /// <remark>
    /// 不支持跨库事务和分布式事务，请注意
    /// </remark>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TransactionAttribute : IAopAttribute
    {
        public TransactionAttribute()
        {

        }

        public TransactionAttribute(Propagation propagation)
        {
            this.Propagation = propagation;
        }

        public TransactionAttribute(Propagation propagation, IsolationLevel isolationLevel)
        {
            this.Propagation = propagation;
            this.IsolationLevel = isolationLevel;
        }

        /// <summary>
        /// 事务传播方式
        /// </summary>
        public Propagation Propagation { get; set; } = Propagation.Required;

        /// <summary>
        /// 事务隔离级别
        /// </summary>
        public IsolationLevel? IsolationLevel { get; set; }
    }
}
