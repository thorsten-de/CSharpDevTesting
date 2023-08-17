using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using ShoppingCartService.BusinessLogic;
using ShoppingCartService.BusinessLogic.Validation;
using ShoppingCartService.Config;
using ShoppingCartService.Controllers;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartService.Models;
using ShoppingCartServiceTests.Builders;
using ShoppingCartServiceTests.Fixtures;
using Xunit;
using static ShoppingCartServiceTests.Builders.ItemBuilder;
using static ShoppingCartServiceTests.Builders.AddressBuilder;
using static ShoppingCartServiceTests.HelperExtensions;

namespace ShoppingCartServiceTests.Controllers
{
    public class ShoppingCartControllerIntegrationTests 
    {
        private readonly ShoppingCartDatabaseSettings _databaseSettings;
        private readonly IMapper _mapper = ConfigureMapper();
        private readonly IShoppingCartRepository _repository = new FakeShoppingCartRepository();

        private const string INVALID_ID = "507f191e810c19729de860ea";
        private const string VALID_ID = "0815-cart";

        [Fact]
        public void GetAll_HasOneCart_returnAllShoppingCartsInformation()
        {

            var cart = new CartBuilder()
                .WithId(null)
                .WithCustomerId("1")
                .WithItems(new List<Item> {CreateItem()})
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
                .WithItems(new List<Item> {CreateItem()})
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

            var actual = target.FindById(INVALID_ID);

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
                .WithItems(new List<Item> {CreateItem()})
                .Build();
            _repository.Create(cart);

            var target = CreateShoppingCartController(_repository);

            var actual = target.CalculateTotals(cart.Id);

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

                Items = new[] {CreateItemDto()}
            });

            Assert.IsType<CreatedAtRouteResult>(result.Result);
            var cartId = ((CreatedAtRouteResult) result.Result).RouteValues["id"].ToString();

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

                Items = new[] {itemDto, CreateItemDto(productId: itemDto.ProductId)}
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

        private ShoppingCartController CreateShoppingCartController(IShoppingCartRepository repository)
        {
            return new(
                new ShoppingCartManager(repository, new AddressValidator(), _mapper,
                    new CheckOutEngine(new ShippingCalculator(), _mapper)), new NullLogger<ShoppingCartController>());
        }


        private class FakeShoppingCartRepository : IShoppingCartRepository
        {
            private Cart _cart;
            public Cart Create(Cart cart)
            {
                cart.Id = VALID_ID;
                _cart = cart;
                return _cart;
            }

            public IEnumerable<Cart> FindAll()
            {
                yield return _cart;
            }

            public Cart FindById(string id) => _cart;

            public void Remove(Cart cart)
            {
                _cart = null;
            }

            public void Remove(string id)
            {
                _cart = null;
            }


            public void Update(string id, Cart cart)
            {
                _cart = cart;
            }
        }
    }
}