using System;
using AutoMapper;
using ShoppingCartService.Controllers.Models;
using ShoppingCartService.DataAccess;
using ShoppingCartService.DataAccess.Entities;

namespace ShoppingCartService.BusinessLogic
{
    public class CouponManager
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IMapper _mapper;

        public CouponManager(ICouponRepository couponRepository, IMapper mapper)
        {
            _couponRepository = couponRepository;
            _mapper = mapper;
        }

        public CouponDto CreateCoupon(CreateCouponDto couponDto)
        {
            var coupon = _mapper.Map<Coupon>(couponDto);
            _couponRepository.Create(coupon);

            return _mapper.Map<CouponDto>(coupon);
        }

        public CouponDto FindCouponById(string id)
        {
            var coupon = _couponRepository.FindById(id);

            return _mapper.Map<CouponDto>(coupon);
        }

        public void DeleteCuponById(string id) => _couponRepository.DeleteById(id);
    }
}