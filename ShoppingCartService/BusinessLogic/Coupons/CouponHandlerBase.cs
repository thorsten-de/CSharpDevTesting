using System;
using ShoppingCartService.BusinessLogic.Exceptions;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartService.BusinessLogic.Coupons
{
    public class CouponHandlerBase
    {
        private readonly Func<CheckoutDto, Coupon, double> _calculateAmountFunc;
        private readonly Func<CheckoutDto, Coupon, bool> _validationFunc;

        protected CouponHandlerBase(Func<CheckoutDto, Coupon, bool> validationFunc,
            Func<CheckoutDto, Coupon, double> calculateAmountFunc)
        {
            _validationFunc = validationFunc;
            _calculateAmountFunc = calculateAmountFunc;
        }

        public bool IsValid(CheckoutDto checkoutDto, Coupon coupon)
        {
            return _validationFunc(checkoutDto, coupon);
        }

        public double CalculateAmount(CheckoutDto checkoutDto, Coupon coupon)
        {
            if (IsValid(checkoutDto, coupon)) return _calculateAmountFunc(checkoutDto, coupon);

            throw new InvalidCouponException("Coupon amount not valid");
        }
    }
}