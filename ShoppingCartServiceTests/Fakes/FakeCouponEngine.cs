using ShoppingCartService.BusinessLogic;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartServiceTests.Fakes
{
    internal class FakeCouponEngine : ICouponEngine
    {
        public double AmountGenerated { get; set; }
        public FakeCouponEngine(double amount) { 
            AmountGenerated = amount;
        }

        public double CalculateDiscount(CheckoutDto checkoutDto, Coupon coupon)
            => AmountGenerated;
    }
}
