using FreeSql;
using OnceMi.Framework.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.IRepository
{
    public class BaseUnitOfWorkManager : UnitOfWorkManager
    {
        public BaseUnitOfWorkManager(IdleBus<IFreeSql> ib) : base(ib.Get())
        {

        }
    }
}
