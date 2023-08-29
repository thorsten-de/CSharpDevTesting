using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Xunit;

using ShoppingCartService.BusinessLogic;
using ShoppingCartService.Controllers;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;

using static ShoppingCartServiceTests.HelperExtensions;
using Moq;

namespace ShoppingCartServiceTests.Controllers
{
    public partial class CouponsControllerUnitTests 
    {
        private readonly IMapper _mapper = ConfigureMapper();

        [Fact]
        public void CreateCoupon_couponCreated()
        {
            var mockRepo = new Mock<ICouponRepository>();
            mockRepo
                .Setup(r => r.Create(It.IsAny<Coupon>()))
                .Callback<Coupon>(coupon =>
                {
                    coupon.Id = "coupon-id";
                });

            var target = new CouponController(new CouponManager(mockRepo.Object, _mapper));

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

            var stubCouponRepository = new Mock<ICouponRepository>();
            stubCouponRepository
                .Setup(r => r.FindById("coupon-id"))
                .Returns(coupon);

            var target = new CouponController(new CouponManager(stubCouponRepository.Object, _mapper));

            var actual = target.FindById("coupon-id");

            var expected = new CouponDto("coupon-id", CouponType.Percentage, 10, coupon.Expiration);
            Assert.Equal(expected, actual.Value);
        }

        [Fact]
        public void FindById_notFound_returnNotFoundResult()
        {
            var stubCouponRepository = new Mock<ICouponRepository>();
            stubCouponRepository
                .Setup(r => r.FindById("coupon-unkwnon"))
                .Returns<Coupon>(null);

            var target = new CouponController(new CouponManager(stubCouponRepository.Object, _mapper));

            var actual = target.FindById("coupon-unknown");

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void Delete_ReturnNoConentAndDeleteItem()
        {
            var mockRepository = new Mock<ICouponRepository>();

            var target = new CouponController(new CouponManager(mockRepository.Object, _mapper));

            var actual = target.DeleteCoupon("coupon-1");
            
            Assert.IsType<NoContentResult>(actual);
            mockRepository.Verify(r => r.DeleteById("coupon-1"));
        }
    }
}