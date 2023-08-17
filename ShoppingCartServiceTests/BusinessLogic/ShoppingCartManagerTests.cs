using Xunit;
using AutoMapper;

using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartServiceTests.Fakes;

using static ShoppingCartServiceTests.HelperExtensions;
using static ShoppingCartServiceTests.Builders.CheckOutDtoBuilder;
using static ShoppingCartServiceTests.Builders.CouponBuilder;

namespace ShoppingCartServiceTests.BusinessLogic
{
    public class ShoppingCartManagerTests
    {
        private readonly IMapper _mapper = ConfigureMapper();
        private readonly IShoppingCartRepository _repository = new FakeShoppingCartRepository();
        private readonly ICouponRepository _couponRepository = new FakeCouponRepository();
        private readonly IAddressValidator _addressValidator = new FakeAddressValidator();

        [Fact]
        public void CalculateTotals_IncludeCouponWithCart()
        {
            ShoppingCartManager target = CreateShoppingCartManager(CreateCheckOutDto(total: 100));
            var coupon = _couponRepository.Create(CreateCoupon(value: 15));

            var result = target.CalculateTotals(FakeShoppingCartRepository.VALID_ID, coupon.Id);

            Assert.Equal(100, result.Total);
            Assert.Equal(15, result.CouponDiscount);
            Assert.Equal(85, result.TotalAfterCoupon);
        }

        private ShoppingCartManager CreateShoppingCartManager(CheckoutDto checkoutDto)
        {
            var cartManager = new ShoppingCartManager(_repository, _addressValidator, _mapper,
                new FakeCheckoutEngine(checkoutDto),
                new CouponEngine(),
                _couponRepository);
            
            cartManager.Create(new CreateCartDto { });
            return cartManager;
        }
    }

}
