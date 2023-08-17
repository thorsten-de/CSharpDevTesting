using AutoMapper;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartServiceTests.Fakes;
using Xunit;

using ShoppingCartServiceTests.Builders;
using System.Collections.Generic;
using ShoppingCartService.DataAccess.Entities;

using static ShoppingCartServiceTests.HelperExtensions;
using static ShoppingCartServiceTests.Builders.CheckOutDtoBuilder;
using static ShoppingCartServiceTests.Builders.ItemBuilder;
using static ShoppingCartServiceTests.Builders.AddressBuilder;

namespace ShoppingCartServiceTests.BusinessLogic
{
    public class ShoppingCartManagerTests
    {
        private readonly IMapper _mapper = ConfigureMapper();
        private readonly IShoppingCartRepository _repository = new FakeShoppingCartRepository();
        private readonly IAddressValidator _addressValidator = new FakeAddressValidator();

        [Fact]
        public void CalculateTotals_IncludeCouponWithCart()
        {
            var checkoutDto = CreateCheckOutDto(total: 100);
            ShoppingCartManager target = CreateShoppingCartManager(checkoutDto);
            target.Create(new CreateCartDto { });

            var result = target.CalculateTotals(FakeShoppingCartRepository.VALID_ID);

            Assert.Equal(100, result.Total);
        }


        private ShoppingCartManager CreateShoppingCartManager(CheckoutDto checkoutDto) => 
            new ShoppingCartManager(_repository, _addressValidator, _mapper, new FakeCheckoutEngine(checkoutDto));


    }

}
