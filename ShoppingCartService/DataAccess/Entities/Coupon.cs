using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ShoppingCartService.Models;

namespace ShoppingCartService.DataAccess.Entities
{
    public class Coupon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public CouponType CouponType { get; set; }
        public double Value { set; get; } = 0;
        public DateTime Expiration { set; get; }

        protected bool Equals(Coupon other)
        {
            return Id == other.Id && CouponType == other.CouponType && Value.Equals(other.Value) && Expiration.Equals(other.Expiration);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coupon) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, (int) CouponType, Value, Expiration);
        }
    }
}