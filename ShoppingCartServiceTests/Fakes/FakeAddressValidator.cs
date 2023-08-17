using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Models;

namespace ShoppingCartServiceTests.Fakes
{
    internal class FakeAddressValidator : IAddressValidator
    {
        public bool ShouldBeValid { get; set; } = true;

        public bool IsValid(Address _) => ShouldBeValid;
    }
}
