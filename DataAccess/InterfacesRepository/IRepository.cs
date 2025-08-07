using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.InterfacesRepository
{
    public interface IRepository<T> where T : class
    {
        // Category
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? fillter = null, string? includeProperties = null);
        T Get(Expression<Func<T, bool>> function, string? includeProperties = null, bool traked = false);
        void Add(T entity);

        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);



    }
}
