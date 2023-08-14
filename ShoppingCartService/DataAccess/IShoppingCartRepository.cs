using System.Collections.Generic;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartService.DataAccess
{
    public interface IShoppingCartRepository
    {
        IEnumerable<Cart> FindAll();
        Cart FindById(string id);
        Cart Create(Cart cart);
        void Update(string id, Cart cart);
        void Remove(Cart cart);
        void Remove(string id);
    }
}