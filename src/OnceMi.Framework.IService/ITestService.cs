using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService
{
    public interface ITestService : IBaseService
    {
        int TestSyncProxyWithResult();

        void TestSyncProxyWithoutResult();

        Task<int> TestAsyncProxy();

        Task TestTaskProxy();
    }
}
