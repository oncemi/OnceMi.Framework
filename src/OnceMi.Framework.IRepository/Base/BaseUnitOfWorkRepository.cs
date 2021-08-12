using FreeSql;
using System;
using System.Linq.Expressions;

namespace OnceMi.Framework.IRepository
{
    public abstract class BaseUnitOfWorkRepository<TEntity, TKey> : BaseRepository<TEntity, TKey> where TEntity : class
    {
        public IdleBus<IFreeSql> DbContainer { get; set; }

        public BaseUnitOfWorkRepository(BaseUnitOfWorkManager uow, Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null)
            : base(uow.Orm, filter, asTable)
        {
            uow.Binding(this);
            this.DbContainer = uow.DbContainer;
        }
    }
}
