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
    public class ShoppingCartControllerIntegrationTests
    {
        private readonly IMapper _mapper = ConfigureMapper();
        private readonly FakeShoppingCartRepository _repository = new FakeShoppingCartRepository();
        private readonly ICouponRepository _couponRepository = new FakeCouponRepository();


        [Fact]
        public void GetAll_HasOneCart_returnAllShoppingCartsInformation()
        {

            var cart = new CartBuilder()
                .WithId(null)
                .WithCustomerId("1")
                .WithItems(new List<Item> { CreateItem() })
                .Build();
            _repository.Create(cart);


            var target = CreateShoppingCartController(_repository);

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

            _repository.Create(cart);

            var target = CreateShoppingCartController(_repository);

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
            var target = CreateShoppingCartController(_repository);

            var actual = target.FindById(FakeShoppingCartRepository.INVALID_ID);

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void CalculateTotals_ShoppingCartNotFound_ReturnNotFound()
        {
            var target = CreateShoppingCartController(_repository);

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
            _repository.Create(cart);

            var target = CreateShoppingCartController(_repository);

            var actual = target.CalculateTotals(cart.Id, FakeCouponRepository.Valid_ID);

            Assert.NotEqual(0.0, actual.Value.Total);
        }

        [Fact]
        public void Create_ValidData_SaveShoppingCartToDB()
        {
            var target = CreateShoppingCartController(_repository);

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

            var value = _repository.FindById(cartId);

            Assert.NotNull(value);
        }

        [Fact]
        public void Create_DuplicateItem_ReturnBadRequestResult()
        {
            var target = CreateShoppingCartController(_repository);

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
            var target = CreateShoppingCartController(_repository);

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

            _repository.Create(cart);

            var target = CreateShoppingCartController(_repository);

            var result = target.DeleteCart(cart.Id);

            var value = _repository.FindById(cart.Id);

            Assert.Null(value);
        }

        [Fact]
        public void CalculateTotals_WithInvalidCoupon_ReturnsBadRequest()
        {
            var cart = new CartBuilder()
                .Build();
            _repository.Create(cart);
            _couponRepository.Create(CreateCoupon(value: -50));

            var target = CreateShoppingCartController(_repository);

            var result = target.CalculateTotals(FakeShoppingCartRepository.VALID_ID, FakeCouponRepository.Valid_ID);
            
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void CalculateTotals_WithExpiredCoupon_ReturnsBadRequest()
        {
            var cart = new CartBuilder()
                .Build();
            _repository.Create(cart);
            _couponRepository.Create(CreateCoupon(expiration: new DateTime(2023,08,08)));

            var target = CreateShoppingCartController(_repository, new FakeDateCouponEngine(new DateTime(2023, 08, 17)));

            var result = target.CalculateTotals(FakeShoppingCartRepository.VALID_ID, FakeCouponRepository.Valid_ID);

            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void AddItemToCard_ValidCartAndItem_AddsItem()
        {
            var cart = new CartBuilder()
                .WithCustomerId("1")
                .Build();

            _repository.Create(cart);

            ShoppingCartController target = CreateShoppingCartController(_repository);
            var newItem = CreateItemDto("added-prod");
            
            var result = target.AddItemToCart(FakeShoppingCartRepository.VALID_ID, newItem);

            Assert.True(_repository.WasUpdateCalled);
            Assert.NotEmpty(_repository.LastSavedCart.Items);
        }

        [Fact]
        public void RemoveItemToCard_ValidCartWithItem_RemovesItem()
        {
            var cart = new CartBuilder()
                .WithCustomerId("1")
                .WithItems(new List<Item> { CreateItem(productId: "prod-1") })
                .Build();

            _repository.Create(cart);

            ShoppingCartController target = CreateShoppingCartController(_repository);
            var newItem = CreateItemDto("added-prod");

            var result = target.RemoveItemFromCart(FakeShoppingCartRepository.VALID_ID, "prod-1");

            Assert.True(_repository.WasUpdateCalled);
            Assert.Empty(_repository.LastSavedCart.Items);

        }


        private ShoppingCartController CreateShoppingCartController(IShoppingCartRepository repository, ICouponEngine couponEngine = null)
        {
            return new(
                new ShoppingCartManager(
                    repository,
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