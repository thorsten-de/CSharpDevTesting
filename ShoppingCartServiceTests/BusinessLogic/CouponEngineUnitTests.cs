using System;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Exceptions;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;
using Xunit;

using static ShoppingCartServiceTests.Builders.CheckOutDtoBuilder;
using static ShoppingCartServiceTests.Builders.CouponBuilder;

namespace ShoppingCartServiceTests.BusinessLogic
{
    public class CouponEngineUnitTests
    {
        [Fact]
        public void CalculateDiscount_CouponIsNull_Return0()
        {
            var target = new CouponEngine();

            var actual = target.CalculateDiscount(CreateCheckOutDto(), null);

            Assert.Equal(0, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void CalculateDiscount_CouponHasAmountValue_ReturnNumber(double amount)
        {
            var target = new CouponEngine();

            var actual = target.CalculateDiscount(CreateCheckOutDto(total: 10), CreateCoupon(value: amount));

            Assert.Equal(amount, actual);
        }

        [Fact]
        public void CalculateDiscount_CouponHasValueGreaterThanShoppingCartTotal_ThrowInvalidCouponException()
        {
            var target = new CouponEngine();

            Assert.Throws<InvalidCouponException>(
                () => target.CalculateDiscount(CreateCheckOutDto(total: 10), CreateCoupon(value: 20)));
        }
        
        [Fact]
        public void CalculateDiscount_CouponHasNegativeValue_ThrowInvalidCouponException()
        {
            var target = new CouponEngine();

            Assert.Throws<InvalidCouponException>(
                () => target.CalculateDiscount(CreateCheckOutDto(total: 10), CreateCoupon(value: -10)));
        }

        [Fact]
        public void CalculateDiscount_CouponOfTypePercentage_ReturnPercentageOfTotal()
        {
            var target = new CouponEngine();

            var actual = target.CalculateDiscount(CreateCheckOutDto(total: 200), CreateCoupon(value: 10, couponType: CouponType.Percentage));

            Assert.Equal(20, actual);
        } 
        
        [Fact]
        public void CalculateDiscount_CouponOfTypePercentageAndNegative_ThrowInvalidCouponException()
        {
            var target = new CouponEngine();

            Assert.Throws<InvalidCouponException>(
                () => target.CalculateDiscount(CreateCheckOutDto(total: 200), CreateCoupon(value: -10, couponType: CouponType.Percentage)));
        } 
        
        [Fact]
        public void CalculateDiscount_CouponOfTypePercentageAndHigherThan100_ThrowInvalidCouponException()
        {
            var target = new CouponEngine();

            Assert.Throws<InvalidCouponException>(
                () => target.CalculateDiscount(CreateCheckOutDto(total: 200), CreateCoupon( value: 101, couponType: CouponType.Percentage)));
        }

        [Fact]
        public void CalculateDiscount_CouponOfTypePercentageAndHigherThanAmount_DoNotThrowInvalidCouponException()
        {
            var target = new CouponEngine();

            var actual = target.CalculateDiscount(CreateCheckOutDto(total: 10), CreateCoupon(value: 50, couponType: CouponType.Percentage));

            Assert.Equal(5, actual);
        }
        
        [Fact]
        public void CalculateDiscount_CouponOfTypeFreeShipping_ReturnShippingCost()
        {
            var target = new CouponEngine();

            var actual = target.CalculateDiscount(CreateCheckOutDto(shippingCost: 10), CreateCoupon(couponType: CouponType.FreeShipping));

            Assert.Equal(10, actual);
        }

        [Theory]
        [InlineData(CouponType.Amount)]
        [InlineData(CouponType.Percentage)]
        [InlineData(CouponType.FreeShipping)]
        public void CalculateDiscount_CouponHasExpired_ThrowCouponExpiredException(CouponType couponType)
        {
            var target = new CouponEngine();

                Assert.Throws<CouponExpiredException>(() => 
                    target.CalculateDiscount(CreateCheckOutDto(), CreateCoupon(couponType:couponType, expiration: DateTime.Now.AddDays(-1))));
        }
    }
}