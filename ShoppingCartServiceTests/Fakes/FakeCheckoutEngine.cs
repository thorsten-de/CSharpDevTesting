using ShoppingCartService.BusinessLogic;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartServiceTests.Fakes
{
    internal class FakeCheckoutEngine : ICheckOutEngine
    {
        private CheckoutDto _checkoutDto;

        public FakeCheckoutEngine(CheckoutDto checkoutDto) { 
            _checkoutDto = checkoutDto;
        }

        public CheckoutDto CalculateTotals(Cart cart) => _checkoutDto;
    }
}
