using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UdemyAuthServer.Core.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);
        //tüm data gelsin üstüne daha where sorgusu yazmayacagız dediğimiz için ıenum. yaptık.IQua. üstüne başka where sorguları yazılabilir.
        Task<IEnumerable<TEntity>> GetAllAsync();
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);

        //state'ini memoryde deleted yapar, savechange yapana kadar işlem gercekleşmez. stateini işaretlemek için async metodu yok
        void Remove(TEntity entity);
        //stateini işaretlemek için async metodu yok
        TEntity Update(TEntity entity);
    }
}
