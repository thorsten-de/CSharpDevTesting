namespace ShoppingCartService.BusinessLogic.Coupons
{
    public class FreeShippingCouponHandler : CouponHandlerBase
    {
        public FreeShippingCouponHandler() : base((_, _) => true, (c, _) => c.ShippingCost)
        {
        }
    }
}