using FreeSql;
using OnceMi.Framework.IRepository.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.IRepository
{
    public abstract class BaseUnitOfWorkRepository<TEntity, TKey> : BaseRepository<TEntity, TKey> where TEntity : class
    {
        public BaseUnitOfWorkRepository(BaseUnitOfWorkManager uow, Expression<Func<TEntity, bool>> filter = null, Func<string, string> asTable = null)
            : base(uow.Orm, filter, asTable)
        {
            uow.Binding(this);
        }
    }
}
