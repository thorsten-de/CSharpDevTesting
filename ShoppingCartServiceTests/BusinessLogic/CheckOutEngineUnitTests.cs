using System.Collections.Generic;
using AutoMapper;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Mapping;
using ShoppingCartService.Models;
using ShoppingCartServiceTests.Builders;
using Xunit;

using static ShoppingCartServiceTests.Builders.AddressBuilder;
using static ShoppingCartServiceTests.Builders.ItemBuilder;

namespace ShoppingCartServiceTests.BusinessLogic
{
    public class CheckOutEngineUnitTests
    {
        private readonly IMapper _mapper;

        public CheckOutEngineUnitTests()
        {
            // Ideally do not write any test related logic here
            // Only infrastructure and environment setup

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));

            _mapper = config.CreateMapper();
        }

        [Theory]
        [InlineData(CustomerType.Standard, 0)]
        [InlineData(CustomerType.Premium, 10)]
        public void CalculateTotals_DiscountBasedOnCustomerType(CustomerType customerType, double expectedDiscount)
        {
            var address = CreateAddress();

            var target = new CheckOutEngine(new ShippingCalculator(address), _mapper);

            var cart = new CartBuilder()
                .WithCustomerType(customerType)
                .WithShippingAddress(address)
                .Build();

            var result = target.CalculateTotals(cart);
            
            
            Assert.Equal(expectedDiscount, result.CustomerDiscount);
        }


        [Theory]
        [InlineData(ShippingMethod.Standard)]
        [InlineData(ShippingMethod.Express)]
        [InlineData(ShippingMethod.Expedited)]
        [InlineData(ShippingMethod.Priority)]
        public void CalculateTotals_StandardCustomer_TotalEqualsCostPlusShipping(ShippingMethod shippingMethod)
        {
            var originAddress = CreateAddress(city: "city 1");
            var destinationAddress = CreateAddress(city: "city 2");

            var target = new CheckOutEngine(new ShippingCalculator(originAddress), _mapper);
            
            var cart = new CartBuilder()
                .WithShippingAddress(destinationAddress)
                .WithShippingMethod(shippingMethod)
                .WithItems(new List<Item>
                {
                    CreateItem(price: 2, quantity:3)
                })
                .Build();
                
            var result = target.CalculateTotals(cart);

            Assert.Equal((2 * 3) + result.ShippingCost, result.Total);
        }

        [Fact]
        public void CalculateTotals_MoreThanOneItem_TotalEqualsCostPlusShipping()
        {
            var originAddress = CreateAddress(city: "city 1");
            var destinationAddress = CreateAddress(city: "city 2");

            var target = new CheckOutEngine(new ShippingCalculator(originAddress), _mapper);

            var cart = new CartBuilder()
                .WithShippingAddress(destinationAddress)
                .WithShippingMethod(ShippingMethod.Standard)
                .WithItems(new List<Item>
                {
                    CreateItem(price: 2, quantity:3),
                    CreateItem(price: 4, quantity:5)
                })
                .Build();


            var result = target.CalculateTotals(cart);

            Assert.Equal((2 * 3) + (4 * 5) + result.ShippingCost, result.Total);
        }

        [Fact]
        public void CalculateTotals_PremiumCustomer_TotalEqualsCostPlusShippingMinusDiscount()
        {
            var originAddress = CreateAddress(city: "city 1");
            var destinationAddress = CreateAddress(city: "city 2");

            var target = new CheckOutEngine(new ShippingCalculator(originAddress), _mapper);

            var cart = new CartBuilder()
                .WithCustomerType(CustomerType.Premium)
                .WithShippingAddress(destinationAddress)
                .WithItems(new List<Item>
                {
                    CreateItem(price: 2, quantity:3)
                })
                .Build();
            var result = target.CalculateTotals(cart);

            Assert.Equal((((2 * 3) + result.ShippingCost) * 0.9), result.Total);
        }
    }
}