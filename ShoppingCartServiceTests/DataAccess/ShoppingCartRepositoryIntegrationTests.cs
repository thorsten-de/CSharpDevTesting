using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Driver;
using ShoppingCartService.Config;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;
using ShoppingCartServiceTests.Builders;
using ShoppingCartServiceTests.Fixtures;
using Xunit;

namespace ShoppingCartServiceTests.DataAccess
{
    [Collection("Dockerized MongoDB collection")]
    public class ShoppingCartRepositoryIntegrationTests : IDisposable
            
    {
        private readonly ShoppingCartDatabaseSettings _databaseSettings;
        private const string Invalid_ID = "507f191e810c19729de860ea";

        public ShoppingCartRepositoryIntegrationTests(DockerMongoFixture fixture)
        {
            _databaseSettings = fixture.GetDatabaseSettings();
        }

        public void Dispose()
        {
            var client = new MongoClient(_databaseSettings.ConnectionString);
            client.DropDatabase(_databaseSettings.DatabaseName);
        }

        [Fact]
        public void FindAll_NoCartsInDB_ReturnEmptyList()
        {
            var target = new ShoppingCartRepository(_databaseSettings);

            var actual = target.FindAll();

            Assert.Empty(actual);
        }

        [Fact]
        public void FindAll_HasTwoCartsInDB_ReturnAllCarts()
        {
            var target = new ShoppingCartRepository(_databaseSettings);

            var cart1 = new CartBuilder().WithId(null).WithCustomerId("1").Build();
            target.Create(cart1);

            var cart2 = new CartBuilder().WithId(null).WithCustomerId("2").Build();
            target.Create(cart2);

            var actual = target.FindAll().ToList();

            var expected = new List<Cart> {cart1, cart2};
            Assert.Equal(expected, actual);
        } 
        
        [Fact]
        public void GetById_hasThreeCartsInDB_returnReturnOnlyCartWithCorrectId()
        {
            var target = new ShoppingCartRepository(_databaseSettings);

            var cart1 = new CartBuilder().WithId(null).WithCustomerId("1").Build();
            target.Create(cart1);

            var cart2 = new CartBuilder().WithId(null).WithCustomerId("2").Build();
            target.Create(cart2);

            var cart3 = new CartBuilder().WithId(null).WithCustomerId("3").Build();
            target.Create(cart3);

            var actual = target.FindById(cart2.Id);

            Assert.Equal(cart2, actual);
        }

        [Fact]
        public void GetById_CartNotFound_ReturnNull()
        {
            var target = new ShoppingCartRepository(_databaseSettings);

            var cart1 = new CartBuilder().WithId(null).WithCustomerId("1").Build();
            target.Create(cart1);

            var cart2 = new CartBuilder().WithId(null).WithCustomerId("2").Build();
            target.Create(cart2);


            var cart3 = new CartBuilder().WithId(null).WithCustomerId("3").Build();
            target.Create(cart3);

            var actual = target.FindById(Invalid_ID);

            Assert.Null(actual);
        }

        [Fact]
        public void Update_CartNotFound_DoNotFail()
        {
            var target = new ShoppingCartRepository(_databaseSettings);
            var cart1 = new CartBuilder().WithId(null).WithCustomerId("1").Build();

            target.Update(Invalid_ID, cart1);
        }
        
        [Fact]
        public void Update_CartFound_UpdateValue()
        {
            var target = new ShoppingCartRepository(_databaseSettings);
            var cart1 = new CartBuilder().WithId(null).WithCustomerId("1").Build();
            target.Create(cart1);
            cart1.CustomerId = "other";

            target.Update(cart1.Id, cart1);

            var result = target.FindById(cart1.Id);

            Assert.Equal("other", cart1.CustomerId);
        }

        [Fact]
        public void Remove_CartFound_RemoveFromDb()
        {
            var target = new ShoppingCartRepository(_databaseSettings);
            var cart1 = new CartBuilder().WithId(null).WithCustomerId("1").Build();
            target.Create(cart1);

            target.Remove(cart1);

            var result = target.FindById(cart1.Id);

            Assert.Null(result);
        } 
        
        [Fact]
        public void RemoveById_CartFound_RemoveFromDb()
        {
            var target = new ShoppingCartRepository(_databaseSettings);
            var cart1 = new CartBuilder().WithId(null).WithCustomerId("1").Build();
            target.Create(cart1);

            target.Remove(cart1.Id);

            var result = target.FindById(cart1.Id);

            Assert.Null(result);
        }
    }
}