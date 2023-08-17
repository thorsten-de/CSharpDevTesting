using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartService.BusinessLogic
{
    public interface ICouponEngine
    {
        double CalculateDiscount(CheckoutDto checkoutDto, Coupon coupon);
    }
}