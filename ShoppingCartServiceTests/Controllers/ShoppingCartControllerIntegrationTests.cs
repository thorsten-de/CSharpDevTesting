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

namespace ShoppingCartServiceTests.Controllers
{
    [Collection("Dockerized MongoDB collection")]
    public class ShoppingCartControllerIntegrationTests : IDisposable
    {
        private readonly ShoppingCartDatabaseSettings _databaseSettings;
        private readonly IMapper _mapper;

        public ShoppingCartControllerIntegrationTests(DockerMongoFixture fixture)
        {
            _databaseSettings = fixture.GetDatabaseSettings();

            _mapper = fixture.Mapper;
        }

        public void Dispose()
        {
            var client = new MongoClient(_databaseSettings.ConnectionString);
            client.DropDatabase(_databaseSettings.DatabaseName);
        }

        [Fact]
        public void GetAll_HasOneCart_returnAllShoppingCartsInformation()
        {
            var repository = new ShoppingCartRepository(_databaseSettings);

            var cart = new CartBuilder()
                .WithId(null)
                .WithCustomerId("1")
                .WithItems(new List<Item> {CreateItem()})
                .Build();
            repository.Create(cart);


            var target = CreateShoppingCartController(repository);

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
            var repository = new ShoppingCartRepository(_databaseSettings);

            var cart = new CartBuilder()
                .WithId(null)
                .WithCustomerId("1")
                .WithItems(new List<Item> {CreateItem()})
                .Build();

            repository.Create(cart);

            var target = CreateShoppingCartController(repository);

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
            var repository = new ShoppingCartRepository(_databaseSettings);

            var target = CreateShoppingCartController(repository);

            var actual = target.FindById("507f191e810c19729de860ea");

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void CalculateTotals_ShoppingCartNotFound_ReturnNotFound()
        {
            var repository = new ShoppingCartRepository(_databaseSettings);

            var target = CreateShoppingCartController(repository);

            var actual = target.CalculateTotals("507f191e810c19729de860ea");

            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public void CalculateTotals_ShippingCartFound_ReturnTotals()
        {
            var repository = new ShoppingCartRepository(_databaseSettings);

            var cart = new CartBuilder()
                .WithId(null)
                .WithItems(new List<Item> {CreateItem()})
                .Build();
            repository.Create(cart);

            var target = CreateShoppingCartController(repository);

            var actual = target.CalculateTotals(cart.Id);

            Assert.NotEqual(0.0, actual.Value.Total);
        }

        [Fact]
        public void Create_ValidData_SaveShoppingCartToDB()
        {
            var repository = new ShoppingCartRepository(_databaseSettings);

            var target = CreateShoppingCartController(repository);

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

            var value = repository.FindById(cartId);

            Assert.NotNull(value);
        }

        [Fact]
        public void Create_DuplicateItem_ReturnBadRequestResult()
        {
            var repository = new ShoppingCartRepository(_databaseSettings);

            var target = CreateShoppingCartController(repository);

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
            var repository = new ShoppingCartRepository(_databaseSettings);

            var target = CreateShoppingCartController(repository);

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

            var repository = new ShoppingCartRepository(_databaseSettings);
            repository.Create(cart);

            var target = CreateShoppingCartController(repository);

            var result = target.DeleteCart(cart.Id);

            var value = repository.FindById(cart.Id);

            Assert.Null(value);
        }

        private ShoppingCartController CreateShoppingCartController(ShoppingCartRepository repository)
        {
            return new(
                new ShoppingCartManager(repository, new AddressValidator(), _mapper,
                    new CheckOutEngine(new ShippingCalculator(), _mapper)), new NullLogger<ShoppingCartController>());
        }
    }
}