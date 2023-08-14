using System;
using System.Runtime.Serialization;

namespace ShoppingCartService.BusinessLogic.Exceptions
{
    public class CouponExpiredException : Exception
    {
        public CouponExpiredException()
        {
        }

        protected CouponExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CouponExpiredException(string? message) : base(message)
        {
        }

        public CouponExpiredException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}