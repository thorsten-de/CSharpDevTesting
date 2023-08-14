using MongoDB.Driver;
using ShoppingCartService.Config;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartService.DataAccess
{
    public class CouponRepository : ICouponRepository
    {
        private readonly IMongoCollection<Coupon> _coupons;
        
        public CouponRepository(IShoppingCartDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _coupons = database.GetCollection<Coupon>("Coupons");
        }

        public Coupon Create(Coupon coupon)
        {
            _coupons.InsertOne(coupon);

            return coupon;
        }

        public Coupon FindById(string id)
        {
            return _coupons.Find(coupon => coupon.Id == id)
                .FirstOrDefault();
        }

        public void DeleteById(string id)
        {
            _coupons.DeleteOne(c => c.Id == id);
        }
    }
}