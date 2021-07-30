using System;
using System.Linq;
using System.Threading;

namespace OnceMi.AspNetCore.Extension
{
    public static class IdleBusExtesion
    {
        static AsyncLocal<string> asyncLocalTenantId = new AsyncLocal<string>();
        public static IdleBus<IFreeSql> ChangeTenant(this IdleBus<IFreeSql> ib, string tenantId)
        {
            asyncLocalTenantId.Value = tenantId;
            return ib;
        }

        /// <summary>
        /// 获取Name为default，或列表中第一个数据库连接
        /// </summary>
        /// <param name="ib"></param>
        /// <returns></returns>
        public static IFreeSql Get(this IdleBus<IFreeSql> ib)
        {
            if (!string.IsNullOrEmpty(asyncLocalTenantId.Value))
            {
                return ib.Get(asyncLocalTenantId.Value);
            }
            string[] keys = ib.GetKeys();
            if (keys == null || keys.Length == 0)
            {
                throw new Exception("Not found in the IdleBus.");
            }
            int defaultIndex = keys.ToList().FindIndex(p => p.Equals("default", StringComparison.OrdinalIgnoreCase));
            if(defaultIndex >= 0)
            {
                return ib.Get(keys[defaultIndex]);
            }
            return ib.Get(keys.First());
        }

        //public static IBaseRepository<T> GetRepository<T>(this IdleBus<IFreeSql> ib) where T : class
        //{
        //    return ib.Get().GetRepository<T>();
        //}
    }
}
