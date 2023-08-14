using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartService.BusinessLogic.Coupons
{
    public class PercentageCouponHandler : CouponHandlerBase
    {
        public PercentageCouponHandler() : base((_, coupon) => coupon.Value <= 100, CalculatePercentage)
        {
        }

        private static double CalculatePercentage(CheckoutDto checkoutDto, Coupon coupon)
        {
            return checkoutDto.Total * coupon.Value / 100;
        }
    }
}