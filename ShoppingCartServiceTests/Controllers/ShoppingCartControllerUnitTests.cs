using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Exceptions;
using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Controllers;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;
using ShoppingCartServiceTests.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static ShoppingCartServiceTests.Builders.AddressBuilder;
using static ShoppingCartServiceTests.Builders.CouponBuilder;
using static ShoppingCartServiceTests.Builders.ItemBuilder;
using static ShoppingCartServiceTests.Builders.CheckOutDtoBuilder;

namespace ShoppingCartServiceTests.Controllers
{
    public class ShoppingCartControllerUnitTests : TestBase
    {

        public ShoppingCartControllerUnitTests()
        {
            _mocker.Use<ILogger<ShoppingCartController>>(new NullLogger<ShoppingCartController>());

        }

        private Cart FakeDefaultCartRepository(params Item[] items)
        {
            var cart = new CartBuilder()
                .WithId("cart-1")
                .WithCustomerId("1")
                .WithItems(items.ToList())
                .Build();

            var fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            fakeCartRepository
                .Setup(r => r.FindById("cart-1"))
                .Returns(cart);
            fakeCartRepository
                .Setup(r => r.FindAll())
                .Returns(new[] { cart });

            return cart;
        }

        [Fact]
        public void GetAll_HasOneCart_returnAllShoppingCartsInformation()
        {
            var cart = FakeDefaultCartRepository(CreateItem());

            var target = _mocker.CreateInstance<ShoppingCartController>();

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
            var cart = FakeDefaultCartRepository(CreateItem());
            
            var target = _mocker.CreateInstance<ShoppingCartController>();

            var actual = target.FindById("cart-1");

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
            var target = _mocker.CreateInstance<ShoppingCartController>();

            var actual = target.FindById("unknwon-cart");

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void CalculateTotals_ShoppingCartNotFound_ReturnNotFound()
        {
            var target = _mocker.CreateInstance<ShoppingCartController>();

            var actual = target.CalculateTotals("");

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void CalculateTotals_ShippingCartFound_ReturnTotals()
        {
            FakeDefaultCartRepository(CreateItem());
            var fakeCheckoutEngine = _mocker.GetMock<ICheckOutEngine>();
            fakeCheckoutEngine
                .Setup(e => e.CalculateTotals(It.IsAny<Cart>()))
                .Returns(CreateCheckOutDto(total: 100));


            var target = _mocker.CreateInstance<ShoppingCartController>();

            var actual = target.CalculateTotals("cart-1", "coupon-1");

            Assert.Equal(100.0, actual.Value.Total);
        }

        [Fact]
        public void Create_ValidData_SaveShoppingCartToDB()
        {
            var _fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            _fakeCartRepository
                .Setup(r => r.Create(It.IsAny<Cart>()))
                .Callback<Cart>(cart => cart.Id = "cart-1")
                .Returns<Cart>(cart => cart);

            var fakeAddressValidator = _mocker.GetMock<IAddressValidator>();
            fakeAddressValidator
                .Setup(v => v.IsValid(It.IsAny<Address>()))
                .Returns(true);

            var target = _mocker.CreateInstance<ShoppingCartController>();

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
            Assert.Equal("cart-1", cartId);
        }

        [Fact]
        public void Create_DuplicateItem_ReturnBadRequestResult()
        {
            var target = _mocker.CreateInstance<ShoppingCartController>();

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
            var target = _mocker.CreateInstance<ShoppingCartController>();

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
            var cart = FakeDefaultCartRepository(CreateItem());
            var _fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();

            var target = _mocker.CreateInstance<ShoppingCartController>();

            var result = target.DeleteCart("cart-1");

            _fakeCartRepository.Verify(r => r.Remove("cart-1"), Times.Once);
        }

        [Fact]
        public void CalculateTotals_WithInvalidCoupon_ReturnsBadRequest()
        {
            var cart = FakeDefaultCartRepository();

            var fakeCouponRegistry = _mocker.GetMock<ICouponRepository>();
            fakeCouponRegistry
                .Setup(r => r.FindById("coupon-1"))
                .Returns(CreateCoupon(value: -50));

            var fakeCouponEngine = _mocker.GetMock<ICouponEngine>();
            fakeCouponEngine
                .Setup(e => e.CalculateDiscount(It.IsAny<CheckoutDto>(), It.IsAny<Coupon>()))
                .Throws(() => new InvalidCouponException());


            var target = _mocker.CreateInstance<ShoppingCartController>();

            var result = target.CalculateTotals("cart-1", "coupon-1");

            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void AddItemToCard_InvalidCartID_ReturnNotFound()
        {
            var _fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();

            ShoppingCartController target = _mocker.CreateInstance<ShoppingCartController>();

            var result = target.AddItemToCart("unknwon-cart", CreateItemDto());

            Assert.IsType<NotFoundResult>(result.Result);
            _fakeCartRepository.Verify(r => r.Update("unknown-cart", It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public void AddItemToCard_ItemNotInCart_CreateNewItem()
        {
            var _fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            _fakeCartRepository
                .Setup(r => r.FindById("cart-1"))
                .Returns(new CartBuilder()
                    .WithItems(new List<Item> { CreateItem(productId: "other") })
                    .Build());

            Cart lastSavedCart = null;
            _fakeCartRepository
                .Setup(r => r.Update("cart-1", It.IsAny<Cart>()))
                .Callback<string, Cart>((_, cart) => lastSavedCart = cart);

            ShoppingCartController target = _mocker.CreateInstance<ShoppingCartController>();
            var newItem = CreateItemDto(productId: "item-1");

            var result = target.AddItemToCart("cart-1", newItem);

            Assert.IsType<OkResult>(result.Result);
            Assert.NotNull(lastSavedCart);

            var actualItemIds = lastSavedCart.Items
                .Select(i => i.ProductId)
                .ToArray();

            Assert.Equal(new[] { "other", "item-1" }, actualItemIds);
        }

        [Fact]
        public void AddItemtoCart_ItemInCart_IncreaseItemQuantity()
        {
            var _fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            _fakeCartRepository
                .Setup(r => r.FindById("cart-1"))
                .Returns(new CartBuilder()
                    .WithItems(new List<Item> { CreateItem(productId: "item-1", quantity: 3) })
                    .Build());

            Cart lastSavedCart = null;
            _fakeCartRepository
                .Setup(r => r.Update("cart-1", It.IsAny<Cart>()))
                .Callback<string, Cart>((_, cart) => lastSavedCart = cart);


            ShoppingCartController target = _mocker.CreateInstance<ShoppingCartController>();
            var newItem = CreateItemDto(productId: "item-1", quantity: 5);

            var result = target.AddItemToCart("cart-1", newItem);

            Assert.IsType<OkResult>(result.Result);
            Assert.NotNull(lastSavedCart);
            Assert.Equal(8u, lastSavedCart.Items.Single().Quantity);
        }

        [Fact]
        public void RemoveItemFromCart_InvalidCartID_ReturnNotFound()
        {
            var _fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            ShoppingCartController target = _mocker.CreateInstance<ShoppingCartController>();

            var result = target.RemoveItemFromCart("unknown-cart", "maybe-knwon-item");

            Assert.IsType<NotFoundResult>(result);
            _fakeCartRepository.Verify(r => r.Update("unknown-cart", It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public void RemoveItemFromCart_ValidCartWithoutValidProductID_ReturnNotFound()
        {
            var _fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            var cart = FakeDefaultCartRepository(CreateItem());

            ShoppingCartController target = _mocker.CreateInstance<ShoppingCartController>();

            var result = target.RemoveItemFromCart("cart-1", "unknwon-item");

            Assert.IsType<NotFoundResult>(result);
            _fakeCartRepository.Verify(r => r.Update("cart-1", It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public void RemoveItemToCard_ValidCartAndProductID_RemoveItemFromCart()
        {
            var _fakeCartRepository = _mocker.GetMock<IShoppingCartRepository>();
            _fakeCartRepository
                .Setup(r => r.FindById("cart-1"))
                .Returns(new CartBuilder()
                    .WithItems(new List<Item> {
                        CreateItem(productId: "prod-1"),
                        CreateItem(productId: "prod-2"),
                        CreateItem(productId: "prod-3")
                    }).Build());

            Cart lastSavedCart = null;
            _fakeCartRepository
                .Setup(r => r.Update("cart-1", It.IsAny<Cart>()))
                .Callback<string, Cart>((_, cart) => lastSavedCart = cart);

            ShoppingCartController target = _mocker.CreateInstance<ShoppingCartController>();

            var result = target.RemoveItemFromCart("cart-1", "prod-2");

            Assert.IsType<OkResult>(result);


            var actualProductIds = lastSavedCart.Items
                .Select(i => i.ProductId)
                .ToArray();

            Assert.Equal(new[] { "prod-1", "prod-3" }, actualProductIds);
        }
    }
}