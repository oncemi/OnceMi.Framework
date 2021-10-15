using FreeSql;
using OnceMi.Framework.Util.Extensions;

namespace OnceMi.Framework.IRepository
{
    public class BaseUnitOfWorkManager : UnitOfWorkManager
    {
        public IdleBus<IFreeSql> DbContainer { get; set; }

        public BaseUnitOfWorkManager(IdleBus<IFreeSql> ib) : base(ib.Get())
        {
            DbContainer = ib;
        }
    }
}
