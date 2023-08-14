using System;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;

namespace ShoppingCartServiceTests.Builders
{
    public class CouponBuilder
    {
        public static Coupon CreateCoupon(
            CouponType couponType = CouponType.Amount,
            double value = 0,
            DateTime? expiration = null
        )
        {
            expiration ??= DateTime.Now.AddDays(1);

            return new Coupon
            {
                CouponType = couponType,
                Value = value,
                Expiration = expiration.Value
            };
        }
    }
}