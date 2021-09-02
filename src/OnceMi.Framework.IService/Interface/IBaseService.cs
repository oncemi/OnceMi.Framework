using FreeSql;
using OnceMi.AspNetCore.AutoInjection;
using OnceMi.Framework.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService
{
    /// <summary>
    /// 继承该接口的Service将被自动注入
    /// </summary>
    [IgnoreDependency]
    public interface IBaseService : IServiceDependency
    {

    }

    [IgnoreDependency]
    public interface IBaseService<TEntity, TKey> : IBaseService
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp);

        ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp);

        TEntity Insert(TEntity entity);

        List<TEntity> Insert(IEnumerable<TEntity> entitys);

        int Update(TEntity entity);

        int Update(IEnumerable<TEntity> entitys);

        TEntity InsertOrUpdate(TEntity entity);

        /// <summary>
        /// 保存实体的指定 ManyToMany/OneToMany 导航属性（完整对比）<para></para>
        /// 场景：在关闭级联保存功能之后，手工使用本方法<para></para>
        /// 例子：保存商品的 OneToMany 集合属性，SaveMany(goods, "Skus")<para></para>
        /// 当 goods.Skus 为空(非null)时，会删除表中已存在的所有数据<para></para>
        /// 当 goods.Skus 不为空(非null)时，添加/更新后，删除表中不存在 Skus 集合属性的所有记录
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="propertyName">属性名</param>
        void SaveMany(TEntity entity, string propertyName);

        int Delete(TEntity entity);

        int Delete(IEnumerable<TEntity> entitys);

        int Delete(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default);

        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default);

        Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default);

        Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<int> DeleteAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default);

        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        TEntity Get(TKey id);

        TEntity Find(TKey id);

        int Delete(TKey id);

        Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntity> FindAsync(TKey id, CancellationToken cancellationToken = default);

        Task<int> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    }
}
