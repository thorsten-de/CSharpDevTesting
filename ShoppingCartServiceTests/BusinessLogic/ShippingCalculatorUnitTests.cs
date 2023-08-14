using System.Collections.Generic;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;
using ShoppingCartServiceTests.Builders;
using Xunit;
using static ShoppingCartServiceTests.Builders.AddressBuilder;
using static ShoppingCartServiceTests.Builders.ItemBuilder;

namespace ShoppingCartServiceTests.BusinessLogic
{
    public class ShippingCalculatorUnitTests
    {
        public static List<object[]> DifferentAddressTypes()
        {
            return new()
            {
                new object[] {CreateAddress(street: "street 1"), CreateAddress(street: "street 2")},
                new object[] {CreateAddress(city: "city 1"), CreateAddress(city: "city 2")},
                new object[] {CreateAddress(country: "country 1"), CreateAddress("country 2")},
            };
        }

        public static List<object[]> AddressTypesWithRates()
        {
            return new()
            {
                new object[]
                {
                    CreateAddress(street: "street 1"),
                    CreateAddress(street: "street 2"),
                    ShippingCalculator.SameCityRate
                },
                new object[]
                {
                    CreateAddress(city: "city 1"),
                    CreateAddress(city: "city 2"),
                    ShippingCalculator.SameCountryRate
                },
                new object[]
                {
                    CreateAddress(country: "country 1"),
                    CreateAddress("country 2"),
                    ShippingCalculator.InternationalShippingRate
                },
            };
        }

        public static List<object[]> ShippingMethodsWithRates()
        {
            return new()
            {
                new object[] {ShippingMethod.Standard, 1.0},
                new object[] {ShippingMethod.Expedited, 1.2},
                new object[] {ShippingMethod.Priority, 2.0},
            };
        }

        [Theory]
        [MemberData(nameof(DifferentAddressTypes))]
        public void CalculateShippingCost_NoItems_Return0(Address source, Address destination)
        {
            var target = new ShippingCalculator(source);

            var cart = new CartBuilder()
                .WithShippingAddress(destination)
                .Build();

            var result = target.CalculateShippingCost(cart);

            Assert.Equal(0, result);
        }

        [Theory]
        [MemberData(nameof(AddressTypesWithRates))]
        public void CalculateShippingCost_StandardShippingOneItemsQuantity1_Return1TimesRate(
            Address source, Address destination, double rate)
        {
            var target = new ShippingCalculator(source);

            var cart = new Cart
            {
                CustomerType = CustomerType.Standard,
                ShippingMethod = ShippingMethod.Standard,
                Items = new List<Item>
                {
                    new() {Quantity = 1}
                },
                ShippingAddress = destination
            };

            var result = target.CalculateShippingCost(cart);

            Assert.Equal(1 * rate, result);
        }

        [Theory]
        [MemberData(nameof(AddressTypesWithRates))]
        public void CalculateShippingCost_StandardShippingOneItemsQuantity5_return5TimesRate(
            Address source, Address destination, double rate)
        {
            var target = new ShippingCalculator(source);

            var cart = new Cart
            {
                CustomerType = CustomerType.Standard,
                ShippingMethod = ShippingMethod.Standard,
                Items = new List<Item>
                {
                    new() {Quantity = 5}
                },
                ShippingAddress = destination
            };

            var result = target.CalculateShippingCost(cart);

            Assert.Equal(5 * rate, result);
        }

        [Theory]
        [MemberData(nameof(AddressTypesWithRates))]
        public void CalculateShippingCost_StandardShippingTwoItems_ReturnSumOfItemsQuantityTimesRate(
            Address source, Address destination, double rate)
        {
            var target = new ShippingCalculator(source);

            var cart = new Cart
            {
                CustomerType = CustomerType.Standard,
                ShippingMethod = ShippingMethod.Standard,
                Items = new List<Item>
                {
                    new() {Quantity = 5},
                    new() {Quantity = 3}
                },
                ShippingAddress = destination
            };

            var result = target.CalculateShippingCost(cart);

            Assert.Equal(8 * rate, result);
        }

        [Theory]
        [MemberData(nameof(ShippingMethodsWithRates))]
        public void CalculateShippingCost_SameCityShippingOneItemsQuantity1_Return1TimesShippingRate(
            ShippingMethod shippingMethod, double shippigRate)
        {
            var address = CreateAddress();

            var target = new ShippingCalculator(address);

            var cart = new CartBuilder()
                .WithShippingMethod(shippingMethod)
                .WithShippingAddress(address)
                .WithItems(new List<Item> {CreateItem(quantity: 1)})
                .Build();

            var result = target.CalculateShippingCost(cart);

            Assert.Equal(1 * ShippingCalculator.SameCityRate * shippigRate, result);
        }

        [Theory]
        [InlineData(ShippingMethod.Standard)]
        [InlineData(ShippingMethod.Expedited)]
        [InlineData(ShippingMethod.Priority)]
        public void CalculateShippingCost_PremiumCustomer_ShippingRate1(ShippingMethod shippingMethod)
        {
            var address = CreateAddress(country: "country 1");

            var target = new ShippingCalculator(address);

            var cart = new Cart
            {
                CustomerType = CustomerType.Premium,
                ShippingMethod = shippingMethod,
                Items = new List<Item>
                {
                    CreateItem(quantity:  1)
                },
                ShippingAddress = CreateAddress(country: "country 2")
            };

            var result = target.CalculateShippingCost(cart);

            Assert.Equal(1 * ShippingCalculator.InternationalShippingRate, result);
        }

        [Fact]
        public void CalculateShippingCost_PremiumCustomerWithExpressShippingMethod_PayShippingRate()
        {
            var address = CreateAddress(country: "country 1");

            var target = new ShippingCalculator(address);

            var cart = new Cart
            {
                CustomerType = CustomerType.Premium,
                ShippingMethod = ShippingMethod.Express,
                Items = new List<Item>
                {
                    CreateItem(quantity:  1)
                },
                ShippingAddress = CreateAddress(country: "country 2")
            };

            var result = target.CalculateShippingCost(cart);

            Assert.Equal(1 * ShippingCalculator.InternationalShippingRate * 2.5, result);
        }

    }
}