using System.Collections.Generic;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartServiceTests.Fakes
{
    internal class FakeShoppingCartRepository : IShoppingCartRepository
    {
        public const string INVALID_ID = "507f191e810c19729de860ea";
        public const string VALID_ID = "0815-cart";
        private Cart _cart;
        public Cart Create(Cart cart)
        {
            cart.Id = VALID_ID;
            _cart = cart;
            return _cart;
        }

        public IEnumerable<Cart> FindAll()
        {
            yield return _cart;
        }

        public Cart FindById(string id) => _cart;

        public void Remove(Cart cart)
        {
            _cart = null;
        }

        public void Remove(string id)
        {
            _cart = null;
        }


        public void Update(string id, Cart cart)
        {
            _cart = cart;
        }
    }
}