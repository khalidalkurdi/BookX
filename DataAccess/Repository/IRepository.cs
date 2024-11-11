using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public interface IRepository<T> where T : class
    {
        // Category
        IEnumerable<T> GetAll(string? includeProperties = null);
        T Get(Expression<Func<T, bool>> function, string? includeProperties = null);
        void Add(T entity);

        void Remove(T entity);
        void RemoveRange(IEnumerable<T>  entity);



    }
}
