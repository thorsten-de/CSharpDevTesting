using Xunit;
using AutoMapper;

using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;

using static ShoppingCartServiceTests.HelperExtensions;
using static ShoppingCartServiceTests.Builders.CheckOutDtoBuilder;
using static ShoppingCartServiceTests.Builders.CouponBuilder;
using Moq;
using ShoppingCartService.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartServiceTests.Builders;

namespace ShoppingCartServiceTests.BusinessLogic
{
    public class ShoppingCartManagerUnitTests : TestBase
    {


        [Fact]
        public void CalculateTotals_IncludeCouponWithCart()
        {
            var fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            fakeCartRepository
                .Setup(r => r.FindById("cart-1"))
                .Returns(new CartBuilder().Build());

            var fakeCouponRepository = _mocker.GetMock<ICouponRepository>();
            fakeCouponRepository
                .Setup(r => r.FindById("coupon-1"))
                .Returns(CreateCoupon(value: 15));

            var fakeCheckoutEngine = _mocker.GetMock<ICheckOutEngine>();
            fakeCheckoutEngine.Setup(
                e => e.CalculateTotals(It.IsAny<Cart>()))
                      .Returns(CreateCheckOutDto(total: 100));

            _mocker.Use<ICouponEngine>(new CouponEngine());

            ShoppingCartManager target = _mocker.CreateInstance<ShoppingCartManager>();

            var result = target.CalculateTotals("cart-1", "coupon-1");

            Assert.Equal(100, result.Total);
            Assert.Equal(15, result.CouponDiscount);
            Assert.Equal(85, result.TotalAfterCoupon);
        }

        [Fact]
        public void CalculateTotals_WithoutCouponCode_HasNoDiscount()
        {
            var fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            fakeCartRepository
                .Setup(r => r.FindById("cart-1"))
                .Returns(new CartBuilder().Build());

            var fakeCheckoutEngine = _mocker.GetMock<ICheckOutEngine>();
            fakeCheckoutEngine.Setup(
                e => e.CalculateTotals(It.IsAny<Cart>()))
                      .Returns(CreateCheckOutDto(total: 100));
            
            _mocker.Use<ICouponEngine>(new CouponEngine());

            ShoppingCartManager target = _mocker.CreateInstance<ShoppingCartManager>();

            var result = target.CalculateTotals("cart-1");

            Assert.Equal(100, result.Total);
            Assert.Equal(0, result.CouponDiscount);
            Assert.Equal(100, result.TotalAfterCoupon);
        }
    }
}
