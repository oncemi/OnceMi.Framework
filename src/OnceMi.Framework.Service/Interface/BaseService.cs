using FreeSql;
using OnceMi.Framework.Entity;
using OnceMi.Framework.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service
{
    public class BaseService<TEntity, TKey> : IBaseService<TEntity, TKey>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly IBaseRepository<TEntity, TKey> _repository;

        public BaseService(IBaseRepository<TEntity, TKey> repository)
        {
            this._repository = repository;
        }

        public ISelect<TEntity> Where(Expression<Func<TEntity, bool>> exp) => _repository.Where(exp);

        public ISelect<TEntity> WhereIf(bool condition, Expression<Func<TEntity, bool>> exp) => _repository.WhereIf(condition, exp);

        public TEntity Insert(TEntity entity) => _repository.Insert(entity);

        public List<TEntity> Insert(IEnumerable<TEntity> entitys) => _repository.Insert(entitys);

        public int Update(TEntity entity) => _repository.Update(entity);

        public int Update(IEnumerable<TEntity> entitys) => _repository.Update(entitys);

        public TEntity InsertOrUpdate(TEntity entity) => _repository.InsertOrUpdate(entity);

        public void SaveMany(TEntity entity, string propertyName) => _repository.SaveMany(entity, propertyName);

        public int Delete(TEntity entity) => _repository.Delete(entity);

        public int Delete(IEnumerable<TEntity> entitys) => _repository.Delete(entitys);

        public int Delete(Expression<Func<TEntity, bool>> predicate) => _repository.Delete(predicate);

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await _repository.InsertAsync(entity, cancellationToken);

        public async Task<List<TEntity>> InsertAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
            => await _repository.InsertAsync(entitys, cancellationToken);

        public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await _repository.UpdateAsync(entity, cancellationToken);

        public async Task<int> UpdateAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
            => await _repository.UpdateAsync(entitys, cancellationToken);

        public async Task<TEntity> InsertOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await _repository.InsertOrUpdateAsync(entity, cancellationToken);

        public async Task SaveManyAsync(TEntity entity, string propertyName, CancellationToken cancellationToken = default)
            => await _repository.SaveManyAsync(entity, propertyName, cancellationToken);

        public async Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await _repository.DeleteAsync(entity, cancellationToken);

        public async Task<int> DeleteAsync(IEnumerable<TEntity> entitys, CancellationToken cancellationToken = default)
            => await _repository.DeleteAsync(entitys, cancellationToken);

        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => await _repository.DeleteAsync(predicate, cancellationToken);

        public TEntity Get(TKey id) => _repository.Get(id);

        public TEntity Find(TKey id) => _repository.Find(id);

        public int Delete(TKey id) => _repository.Delete(id);

        public async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
            => await _repository.GetAsync(id, cancellationToken);

        public async Task<TEntity> FindAsync(TKey id, CancellationToken cancellationToken = default)
            => await _repository.FindAsync(id, cancellationToken);

        public async Task<int> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
            => await _repository.DeleteAsync(id, cancellationToken);
    }
}
