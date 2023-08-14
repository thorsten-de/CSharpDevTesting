using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartServiceTests.Builders
{
    /// <summary>
    /// This is an example for using the Object Mother pattern
    /// You can use 'using static' to make your code more readable.
    /// </summary>

    public class ItemBuilder
    {
        public static Item CreateItem(
            string productId = "prod-1",
            string productName = "product 1",
            double price = 10.0,
            uint quantity = 1)
        {

            return new()
            {
                ProductId = productId,
                ProductName = productName,
                Price = price,
                Quantity = quantity
            };
        }

        public static ItemDto CreateItemDto(
            string productId = "prod-1",
            string productName = "product 1",
            double price = 10.0,
            uint quantity = 1)
        {

            return new ItemDto(productId, productName, price, quantity);
        }
    }
}