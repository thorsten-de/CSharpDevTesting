using ShoppingCartService.BusinessLogic.Validation;
using Xunit;
using static ShoppingCartServiceTests.Builders.AddressBuilder;


namespace ShoppingCartServiceTests.BusinessLogic.Validation
{
    public class AddressValidatorUnitTests
    {
        [Fact]
        public void IsValid_doesNotHaveCountry_returnFalse()
        {
            var address = CreateAddress(country: null);


            var target = new AddressValidator();

            var result = target.IsValid(address);

            Assert.False(result);
        }

        [Fact]
        public void IsValid_doesNotHaveCity_returnFalse()
        {
            var address = CreateAddress(city: null);

            var target = new AddressValidator();

            var result = target.IsValid(address);

            Assert.False(result);
        }

        [Fact]
        public void IsValid_doesNotHaveStreet_returnFalse()
        {
            var address = CreateAddress(street: null);

            var target = new AddressValidator();

            var result = target.IsValid(address);

            Assert.False(result);
        }

        [Fact]
        public void IsValid_validValues_returnTrue()
        {
            var address = CreateAddress(country: "country-1", city: "city-1", street: "street");

            var target = new AddressValidator();

            var result = target.IsValid(address);

            Assert.True(result);
        }
    }
}
