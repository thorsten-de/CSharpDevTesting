namespace ShoppingCartService.Models
{
    public record Address
    {
        public string Country { get; init; }
        public string City { get; init; }
        public string Street { get; init; }
    }
}