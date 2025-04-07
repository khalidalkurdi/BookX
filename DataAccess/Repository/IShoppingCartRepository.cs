using Models;

namespace DataAccess.Repository
{
    public interface IShoppingCartRepository :IRepository<ShoppingCart>
    {
      
        void Update(ShoppingCart entity);
    }
}
