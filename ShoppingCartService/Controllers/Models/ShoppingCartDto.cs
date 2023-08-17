using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ShoppingCartService.Models;

namespace ShoppingCartService.Controllers.Models
{
    public record ItemDto([Required] string ProductId, [Required] string ProductName, double Price, uint Quantity = 1);

    public record CreateCartDto
    {
        [Required] public CustomerDto Customer { get; init; }
        public IEnumerable<ItemDto> Items { get; init; } = Enumerable.Empty<ItemDto>();
        public ShippingMethod ShippingMethod { get; init; }
    }

    public record ShoppingCartDto
    {
        public string Id { get; init; }
        public string CustomerId { get; init; }
        public CustomerType CustomerType { get; init; }
        public ShippingMethod ShippingMethod { get; init; }
        public Address ShippingAddress { get; init; }
        public IEnumerable<ItemDto> Items { get; init; } = Enumerable.Empty<ItemDto>();

        public virtual bool Equals(ShoppingCartDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && CustomerId == other.CustomerId && CustomerType == other.CustomerType &&
                   ShippingMethod == other.ShippingMethod && Equals(ShippingAddress, other.ShippingAddress) &&
                   Items.SequenceEqual(other.Items);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, CustomerId, (int)CustomerType, (int)ShippingMethod, ShippingAddress, Items);
        }
    }

    public record CheckoutDto(
        ShoppingCartDto ShoppingCart,
        double ShippingCost,
        double CustomerDiscount,
        double Total)
    {
        public double CouponDiscount { get; set; } = 0;
    }
}