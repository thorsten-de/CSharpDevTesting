using ShoppingCartService.Controllers.Models;

namespace ShoppingCartServiceTests.Builders
{
    public class CheckOutDtoBuilder
    {
        public static CheckoutDto CreateCheckOutDto(
            ShoppingCartDto shoppingCart = null,
            double shippingCost = 0,
            double customerDiscount = 0,
            double total = 0
        )
        {
            return new(shoppingCart, shippingCost, customerDiscount, total);
        }
    }
}