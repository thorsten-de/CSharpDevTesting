using System;
using System.Runtime.Serialization;

namespace ShoppingCartService.BusinessLogic.Exceptions
{
    public class InvalidCouponException : Exception
    {
        public InvalidCouponException()
        {
        }

        protected InvalidCouponException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidCouponException(string? message) : base(message)
        {
        }

        public InvalidCouponException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}