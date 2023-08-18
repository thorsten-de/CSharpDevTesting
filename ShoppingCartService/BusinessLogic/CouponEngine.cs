using System;
using System.Collections.Generic;
using ShoppingCartService.BusinessLogic.Coupons;
using ShoppingCartService.BusinessLogic.Exceptions;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;

namespace ShoppingCartService.BusinessLogic
{
    public class CouponEngine : ICouponEngine
    {
        public virtual DateTime GetCurrentTime() => DateTime.Now;

        private static readonly Dictionary<CouponType, CouponHandlerBase> CouponHandlers = new()
        {
            {CouponType.Amount, new AmountCouponHandler()},
            {CouponType.Percentage, new PercentageCouponHandler()},
            {CouponType.FreeShipping, new FreeShippingCouponHandler()}
        };

        public double CalculateDiscount(CheckoutDto checkoutDto, Coupon coupon)
        {
            if (coupon == null) return 0;

            if (coupon.Value < 0) throw new InvalidCouponException("Coupon amount cannot be negative");

            if (coupon.Expiration < GetCurrentTime())
                throw new CouponExpiredException("Cannot apply coupon - coupon expiration date has passed");

            return CouponHandlers[coupon.CouponType].CalculateAmount(checkoutDto, coupon);
        }
    }
}