using Microsoft.Extensions.Logging;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Util.Date;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.Repository
{
    public class DemoRepository : BaseUnitOfWorkRepository<Entity.Admin.Config, long>, IDemoRepository
    {
        private readonly ILogger<DemoRepository> _logger;
        private readonly IFreeSql _db;

        public DemoRepository(BaseUnitOfWorkManager uow
            , ILogger<DemoRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = base.Orm;
        }

        public async Task<List<Entity.Admin.Config>> GetAllConfigs()
        {
            #region 当前库读写

            Entity.Admin.Config config = new Entity.Admin.Config()
            {
                Key = TimeUtil.Timestamp().ToString(),
                Content = TimeUtil.Timestamp().ToString(),
                Description = "当前库",
                CreatedUserId = 1,
                CreatedTime = DateTime.Now,
            };
            int count = await _db.Insert(config).ExecuteAffrowsAsync();

            //查询当前库
            var result1 = await _db.Select<Entity.Admin.Config>().Where(p => !p.IsDeleted).ToListAsync();

            #endregion

            #region 其他库读写

            #region 查询其它库错误的示范

            //为什么以下两种方式是错误的？
            //切勿使用using或Dispose手动去释放IdleBus中管理的对象。IdleBus将自动管理你的对象。

            //错误示范1：
            //using (IFreeSql db = this.DbContainer.Get("mysql"))
            //{
            //    //然后查询
            //    var result2 = await db.Select<Configs>().Where(p => !p.IsDeleted).ToListAsync();
            //    result1.AddRange(result2);
            //}

            //错误示范2：
            //IFreeSql db = this.DbContainer.Get("mysql");
            //var result2 = await db.Select<Configs>().Where(p => !p.IsDeleted).ToListAsync();
            //result1.AddRange(result2);
            //db.Dispose();

            #endregion

            //正确的查询其它库
            //先获取连接字符串配置中，Name中mysql为连接字符串
            IFreeSql db2 = this.DbContainer.Get("mysql");
            //写
            Entity.Admin.Config config2 = new Entity.Admin.Config()
            {
                Key = TimeUtil.Timestamp().ToString(),
                Content = TimeUtil.Timestamp().ToString(),
                Description = "其他库",
                CreatedUserId = 1,
                CreatedTime = DateTime.Now,
            };
            count = await db2.Insert(config2).ExecuteAffrowsAsync();
            //读
            var result2 = await db2.Select<Entity.Admin.Config>().Where(p => !p.IsDeleted).ToListAsync();
            result1.AddRange(result2);

            #endregion

            return result1;
        }
    }
}
