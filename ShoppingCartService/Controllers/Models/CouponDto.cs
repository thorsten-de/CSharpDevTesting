using System;
using ShoppingCartService.Models;

namespace ShoppingCartService.Controllers.Models
{
    public record CreateCouponDto(CouponType CouponType, double Value, DateTime Expiration);
    public record CouponDto(string Id, CouponType CouponType, double Value, DateTime Expiration);

}