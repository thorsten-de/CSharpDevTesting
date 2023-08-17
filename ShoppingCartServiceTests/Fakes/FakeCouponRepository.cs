using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartServiceTests.Fakes
{
    class FakeCouponRepository : ICouponRepository
    {
        public const string Valid_ID = "coupon-0815";
        public const string Invalid_ID = "507f191e810c19729de860ea";

        private Coupon _coupon;

        public Coupon Create(Coupon coupon)
        {
            coupon.Id = Valid_ID;
            _coupon = coupon;
            return _coupon;
        }

        public void DeleteById(string id)
        {
            _coupon = null;
        }

        public Coupon FindById(string id) => _coupon;
    }

}