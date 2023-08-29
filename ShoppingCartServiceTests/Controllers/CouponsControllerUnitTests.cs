using System;
using Microsoft.AspNetCore.Mvc;
using Xunit;

using ShoppingCartService.BusinessLogic;
using ShoppingCartService.Controllers;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;

using Moq;

namespace ShoppingCartServiceTests.Controllers
{
    public partial class CouponsControllerUnitTests : TestBase
    {

        [Fact]
        public void CreateCoupon_couponCreated()
        {
            var mockRepo = _mocker.GetMock<ICouponRepository>();
            mockRepo
                .Setup(r => r.Create(It.IsAny<Coupon>()))
                .Callback<Coupon>(coupon =>
                {
                    coupon.Id = "coupon-id";
                });

            var target = _mocker.CreateInstance<CouponController>();

            var expiration = DateTime.Now.ToUniversalTime().Date;
            var createCouponDto = new CreateCouponDto(
                CouponType: CouponType.Amount,
                Value: 10,
                Expiration: expiration
            );

            var result = target.CreateCoupon(createCouponDto);

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            
            var couponId = ((CreatedAtRouteResult)result.Result).RouteValues["id"].ToString();
            Assert.Equal("coupon-id", couponId);
        }

        [Fact]
        public void FindById_HasOneCouponWithSameId_returnAllCouponInformation()
        {
            var coupon = new Coupon
            {
                Id = "coupon-id",
                CouponType = CouponType.Percentage,
                Value = 10
            };

            var stubCouponRepository = _mocker.GetMock<ICouponRepository>();
            stubCouponRepository
                .Setup(r => r.FindById("coupon-id"))
                .Returns(coupon);

            var target = _mocker.CreateInstance<CouponController>();

            var actual = target.FindById("coupon-id");

            var expected = new CouponDto("coupon-id", CouponType.Percentage, 10, coupon.Expiration);
            Assert.Equal(expected, actual.Value);
        }

        [Fact]
        public void FindById_notFound_returnNotFoundResult()
        {
            var stubCouponRepository = _mocker.GetMock<ICouponRepository>();
            stubCouponRepository
                .Setup(r => r.FindById("coupon-unkwnon"))
                .Returns<Coupon>(null);

            var target = _mocker.CreateInstance<CouponController>();

            var actual = target.FindById("coupon-unknown");

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void Delete_ReturnNoConentAndDeleteItem()
        {
            var mockRepository = _mocker.GetMock<ICouponRepository>();

            var target = _mocker.CreateInstance<CouponController>();

            var actual = target.DeleteCoupon("coupon-1");
            
            Assert.IsType<NoContentResult>(actual);
            mockRepository.Verify(r => r.DeleteById("coupon-1"));
        }
    }
}