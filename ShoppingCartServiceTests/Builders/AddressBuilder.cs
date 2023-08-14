using ShoppingCartService.Models;

namespace ShoppingCartServiceTests.Builders
{
    /// <summary>
    /// This is an example for using the Object Mother pattern
    /// You can use 'using static' to make your code more readable.
    /// </summary>
    public class AddressBuilder
    {
        public static Address CreateAddress(
            string country = "country 1",
            string city = "city 1",
            string street = "street 1"
        )
        {
            return new Address
            {
                Country = country,
                City = city,
                Street = street
            };
        }
    }
}