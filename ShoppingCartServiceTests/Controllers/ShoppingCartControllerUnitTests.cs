using System;
using System.Collections.Generic;
using System.Linq;

using AutoMapper;

using Xunit;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Controllers;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;

using ShoppingCartServiceTests.Builders;
using ShoppingCartServiceTests.Fakes;

using static ShoppingCartServiceTests.Builders.ItemBuilder;
using static ShoppingCartServiceTests.Builders.AddressBuilder;
using static ShoppingCartServiceTests.Builders.CouponBuilder;
using static ShoppingCartServiceTests.HelperExtensions;

namespace ShoppingCartServiceTests.Controllers
{
    public class ShoppingCartControllerUnitTests
    {
        private readonly IMapper _mapper = ConfigureMapper();
        private readonly FakeShoppingCartRepository _fakeCartRepository = new FakeShoppingCartRepository();
        private readonly ICouponRepository _couponRepository = new FakeCouponRepository();


        [Fact]
        public void GetAll_HasOneCart_returnAllShoppingCartsInformation()
        {

            var cart = new CartBuilder()
                .WithId(null)
                .WithCustomerId("1")
                .WithItems(new List<Item> { CreateItem() })
                .Build();
            _fakeCartRepository.Create(cart);


            var target = CreateShoppingCartController();

            var actual = target.GetAll();

            var cartItem = cart.Items[0];
            var expected =
                new ShoppingCartDto
                {
                    Id = cart.Id,
                    CustomerId = cart.CustomerId,
                    CustomerType = cart.CustomerType,
                    ShippingAddress = cart.ShippingAddress,
                    ShippingMethod = cart.ShippingMethod,
                    Items = new List<ItemDto>
                    {
                        new(ProductId: cartItem.ProductId,
                            ProductName: cartItem.ProductName,
                            Price: cartItem.Price,
                            Quantity: cartItem.Quantity
                        )
                    }
                };

            Assert.Equal(expected, actual.Single());
        }

        [Fact]
        public void FindById_HasOneCartWithSameId_returnAllShoppingCartsInformation()
        {
            var cart = new CartBuilder()
                .WithId(null)
                .WithCustomerId("1")
                .WithItems(new List<Item> { CreateItem() })
                .Build();

            _fakeCartRepository.Create(cart);

            var target = CreateShoppingCartController();

            var actual = target.FindById(cart.Id);

            var cartItem = cart.Items[0];
            var expected =
                new ShoppingCartDto
                {
                    Id = cart.Id,
                    CustomerId = cart.CustomerId,
                    CustomerType = cart.CustomerType,
                    ShippingAddress = cart.ShippingAddress,
                    ShippingMethod = cart.ShippingMethod,
                    Items = new List<ItemDto>
                    {
                        new(ProductId: cartItem.ProductId,
                            ProductName: cartItem.ProductName,
                            Price: cartItem.Price,
                            Quantity: cartItem.Quantity
                        )
                    }
                };

            Assert.Equal(expected, actual.Value);
        }


        [Fact]
        public void FindById_ItemNotFound_returnNotFoundResult()
        {
            var target = CreateShoppingCartController();

            var actual = target.FindById(FakeShoppingCartRepository.INVALID_ID);

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void CalculateTotals_ShoppingCartNotFound_ReturnNotFound()
        {
            var target = CreateShoppingCartController();

            var actual = target.CalculateTotals("");

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void CalculateTotals_ShippingCartFound_ReturnTotals()
        {
            var cart = new CartBuilder()
                .WithId(null)
                .WithItems(new List<Item> { CreateItem() })
                .Build();
            _fakeCartRepository.Create(cart);

            var target = CreateShoppingCartController();

            var actual = target.CalculateTotals(cart.Id, FakeCouponRepository.Valid_ID);

            Assert.NotEqual(0.0, actual.Value.Total);
        }

        [Fact]
        public void Create_ValidData_SaveShoppingCartToDB()
        {
            var target = CreateShoppingCartController();

            var result = target.Create(new CreateCartDto
            {
                Customer = new CustomerDto
                {
                    Address = CreateAddress(),
                },

                Items = new[] { CreateItemDto() }
            });

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            var cartId = ((CreatedAtRouteResult)result.Result).RouteValues["id"].ToString();

            var value = _fakeCartRepository.FindById(cartId);

            Assert.NotNull(value);
        }

        [Fact]
        public void Create_DuplicateItem_ReturnBadRequestResult()
        {
            var target = CreateShoppingCartController();

            var itemDto = CreateItemDto();
            var result = target.Create(new CreateCartDto
            {
                Customer = new CustomerDto
                {
                    Address = CreateAddress(),
                },

                Items = new[] { itemDto, CreateItemDto(productId: itemDto.ProductId) }
            });

            Assert.IsType<BadRequestResult>(result.Result);
        }

        public static List<object[]> InvalidAddresses()
        {
            return new()
            {
                new object[] {null},
                new object[] {CreateAddress(country: null)},
                new object[] {CreateAddress(city: null)},
                new object[] {CreateAddress(street: null)},
            };
        }

        [Theory]
        [MemberData(nameof(InvalidAddresses))]
        public void Create_InValidAddress_ReturnBadRequestResult(Address address)
        {
            var target = CreateShoppingCartController();

            var result = target.Create(new CreateCartDto
            {
                Customer = new CustomerDto
                {
                    Address = address
                },
                Items = new[] { CreateItemDto() }
            });

            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void Delete_ValidData_RemoveShoppingCartToDB()
        {
            var cart = new CartBuilder()
                .WithId(null)
                .WithCustomerId("1")
                .WithItems(new List<Item> { CreateItem() })
                .Build();

            _fakeCartRepository.Create(cart);

            var target = CreateShoppingCartController();

            var result = target.DeleteCart(cart.Id);

            var value = _fakeCartRepository.FindById(cart.Id);

            Assert.Null(value);
        }

        [Fact]
        public void CalculateTotals_WithInvalidCoupon_ReturnsBadRequest()
        {
            var cart = new CartBuilder()
                .Build();
            _fakeCartRepository.Create(cart);
            _couponRepository.Create(CreateCoupon(value: -50));

            var target = CreateShoppingCartController();

            var result = target.CalculateTotals(FakeShoppingCartRepository.VALID_ID, FakeCouponRepository.Valid_ID);
            
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void CalculateTotals_WithExpiredCoupon_ReturnsBadRequest()
        {
            var cart = new CartBuilder()
                .Build();
            _fakeCartRepository.Create(cart);
            _couponRepository.Create(CreateCoupon(expiration: new DateTime(2023,08,08)));

            var target = CreateShoppingCartController(new FakeDateCouponEngine(new DateTime(2023, 08, 17)));

            var result = target.CalculateTotals(FakeShoppingCartRepository.VALID_ID, FakeCouponRepository.Valid_ID);

            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void AddItemToCard_InvalidCartID_ReturnNotFound()
        {
            ShoppingCartController target = CreateShoppingCartController();

            var result = target.AddItemToCart("unknwon-cart", CreateItemDto());

            Assert.IsType<NotFoundResult>(result.Result);
            Assert.False(_fakeCartRepository.WasUpdateCalled);
        }

        [Fact]
        public void AddItemToCard_ItemNotInCart_CreateNewItem()
        {
            var cart = new CartBuilder()
                .WithItems(new List<Item> { CreateItem(productId: "other")})
                .Build();

            _fakeCartRepository.Create(cart);

            ShoppingCartController target = CreateShoppingCartController();
            var newItem = CreateItemDto(productId: "item-1");
            
            var result = target.AddItemToCart(FakeShoppingCartRepository.VALID_ID, newItem);

            Assert.IsType<OkResult>(result.Result);

            var actualItemIds = _fakeCartRepository.LastSavedCart.Items
                .Select(i => i.ProductId)
                .ToArray();

            Assert.Equal(new[] { "other", "item-1" }, actualItemIds);
        }

        [Fact]
        public void AddItemtoCart_ItemInCart_IncreaseItemQuantity()
        {
            var cart = new CartBuilder()
                .WithItems(new List<Item> { CreateItem(productId: "item-1", quantity: 3) })
                .Build();

            _fakeCartRepository.Create(cart);

            ShoppingCartController target = CreateShoppingCartController();
            var newItem = CreateItemDto(productId: "item-1", quantity: 5);

            var result = target.AddItemToCart(FakeShoppingCartRepository.VALID_ID, newItem);

            Assert.IsType<OkResult>(result.Result);

            Assert.Equal(8u, _fakeCartRepository.LastSavedCart.Items.Single().Quantity);
        }

        [Fact]
        public void RemoveItemFromCart_InvalidCartID_ReturnNotFound()
        {
            ShoppingCartController target = CreateShoppingCartController();

            var result = target.RemoveItemFromCart("unknown-cart", "maybe-knwon-item");

            Assert.IsType<NotFoundResult>(result);
            Assert.False(_fakeCartRepository.WasUpdateCalled);
        }

        [Fact]
        public void RemoveItemFromCart_ValidCartWithoutValidProductID_ReturnNotFound()
        {
            var cart = new CartBuilder().Build();
            _fakeCartRepository.Create(cart);

            ShoppingCartController target = CreateShoppingCartController();

            var result = target.RemoveItemFromCart(FakeShoppingCartRepository.VALID_ID, "unknwon-item");

            Assert.IsType<NotFoundResult>(result);
            Assert.False(_fakeCartRepository.WasUpdateCalled);
        }

        [Fact]
        public void RemoveItemToCard_ValidCartAndProductID_RemoveItemFromCart()
        {
            var cart = new CartBuilder()
                .WithCustomerId("1")
                .WithItems(new List<Item> { 
                    CreateItem(productId: "prod-1"),
                    CreateItem(productId: "prod-2"),
                    CreateItem(productId: "prod-3") 
                })
                .Build();
            _fakeCartRepository.Create(cart);

            ShoppingCartController target = CreateShoppingCartController();

            var result = target.RemoveItemFromCart(FakeShoppingCartRepository.VALID_ID, "prod-2");

            Assert.IsType<OkResult>(result);

            var actualProductIds = _fakeCartRepository.LastSavedCart.Items
                .Select(i => i.ProductId)
                .ToArray();

            Assert.Equal(new[] { "prod-1", "prod-3" }, actualProductIds);
        }


        private ShoppingCartController CreateShoppingCartController(ICouponEngine couponEngine = null)
        {
            return new(
                new ShoppingCartManager(
                    _fakeCartRepository,
                    new AddressValidator(),
                    _mapper,
                    new CheckOutEngine(new ShippingCalculator(), _mapper),
                    couponEngine ?? new CouponEngine(),
                    _couponRepository),
                new NullLogger<ShoppingCartController>());
        }

        class FakeDateCouponEngine: CouponEngine
        {
            private readonly DateTime _fakeNow;

            public FakeDateCouponEngine(DateTime fakeNow)
            {
                _fakeNow = fakeNow;
            }

            public override DateTime GetCurrentTime() => _fakeNow;
        }
    }
}