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

using ShoppingCartServiceTests.Fakes;

using static ShoppingCartServiceTests.HelperExtensions;

namespace ShoppingCartServiceTests.Controllers
{
    public partial class CouponsControllerIntegrationTests 
    {
        private readonly IMapper _mapper = ConfigureMapper();
        private readonly ICouponRepository _repository = new FakeCouponRepository();

        [Fact]
        public void CreateCoupon_couponCreated()
        {
            var target = new CouponController(new CouponManager(_repository, _mapper));

            var expiration = DateTime.Now.ToUniversalTime().Date;
            var createCouponDto = new CreateCouponDto(
                CouponType: CouponType.Amount,
                Value: 10,
                Expiration: expiration
            );

            var result = target.CreateCoupon(createCouponDto);

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            var couponId = ((CreatedAtRouteResult) result.Result).RouteValues["id"].ToString();

            var actual = _repository.FindById(couponId);

            var expected = new Coupon
            {
                Id = couponId, 
                CouponType = CouponType.Amount, 
                Value = 10,
                Expiration = expiration
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindById_HasOneCartWithSameId_returnAllShoppingCartsInformation()
        {

            var coupon = new Coupon
            {
                CouponType = CouponType.Percentage,
                Value = 10
            };

            _repository.Create(coupon);

            var target = new CouponController(new CouponManager(_repository, _mapper));

            var actual = target.FindById(coupon.Id);


            var expected = new CouponDto(coupon.Id, CouponType.Percentage, 10, coupon.Expiration);

            Assert.Equal(expected, actual.Value);
        }

        [Fact]
        public void FindById_notFound_returnNotFoundResult()
        {
            var target = new CouponController(new CouponManager(_repository, _mapper));

            var actual = target.FindById(FakeCouponRepository.Invalid_ID);

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void Delete_ReturnNoConentAndDeleteItem()
        {
            var coupon = new Coupon
            {
                CouponType = CouponType.Percentage,
                Value = 10
            };

            _repository.Create(coupon);

            var target = new CouponController(new CouponManager(_repository, _mapper));

            var actual = target.DeleteCoupon(coupon.Id);
            Assert.IsType<NoContentResult>(actual);
            var couponFindResult = target.FindById(coupon.Id);

            Assert.IsType<NotFoundResult>(couponFindResult.Result);
        }
    }
}