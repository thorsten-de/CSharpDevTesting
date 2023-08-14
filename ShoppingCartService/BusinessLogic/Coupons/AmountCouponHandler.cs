using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartService.BusinessLogic.Coupons
{
    public class AmountCouponHandler : CouponHandlerBase
    {
        public AmountCouponHandler() : base(IsAmountValid, (_, coupon) => coupon.Value)
        {
        }

        private static bool IsAmountValid(CheckoutDto checkoutDto, Coupon coupon)
        {
            return coupon.Value <= checkoutDto.Total;
        }
    }
}