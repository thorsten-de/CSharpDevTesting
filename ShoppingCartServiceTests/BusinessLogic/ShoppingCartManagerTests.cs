using AutoMapper;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartServiceTests.Fakes;
using Xunit;


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
            var checkoutDto = CreateCheckOutDto(total: 100);

            ShoppingCartManager target = CreateShoppingCartManager(checkoutDto);
            target.Create(new CreateCartDto { });
            var coupon = _couponRepository.Create(CreateCoupon(value: 15));

            var result = target.CalculateTotals(FakeShoppingCartRepository.VALID_ID, coupon.Id);

            Assert.Equal(100, result.Total);
            Assert.Equal(15, result.CouponDiscount);
        }


        private ShoppingCartManager CreateShoppingCartManager(CheckoutDto checkoutDto) => 
            new ShoppingCartManager(_repository, _addressValidator, _mapper, 
                new FakeCheckoutEngine(checkoutDto),
                new CouponEngine(),
                _couponRepository);


    }

}
