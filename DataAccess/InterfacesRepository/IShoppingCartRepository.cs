using Models;

namespace DataAccess.InterfacesRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {

        void Update(ShoppingCart entity);
    }
}
